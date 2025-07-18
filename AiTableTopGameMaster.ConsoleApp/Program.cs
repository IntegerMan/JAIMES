using System.Globalization;
using AiTableTopGameMaster.Adventures.IslandAdventureDemo;
using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Commands;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using AiTableTopGameMaster.ConsoleApp.Settings;
using AiTableTopGameMaster.Core;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Spectre.Console;
using Spectre.Console.Cli;
using Serilog;

// Create a custom IAnsiConsole that logs output to Serilog
IAnsiConsole console = new LoggingConsoleWrapper(AnsiConsole.Console);

try
{
    // Render a header
    console.Write(new FigletText("AIGM")
        .Justify(Justify.Left)
        .Color(Color.Green));
    console.MarkupLine("[blue]by[/] [cyan]Matt Eland[/]");
    console.MarkupLine("[blue]An AI-powered tabletop game master[/]");
    console.WriteLine();
    
    // Configure Serilog for logging
    DateTimeOffset now = DateTimeOffset.Now;
    DateOnly today = DateOnly.FromDateTime(now.Date);
    Guid fileId = Guid.CreateVersion7(now);
    string logFilePath = Path.Combine(Environment.CurrentDirectory, "Logs", $"{today:O}-{fileId}.log");
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File(logFilePath,
            rollingInterval: RollingInterval.Infinite,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            Path.Combine(Environment.CurrentDirectory, "Logs", $"{today:O}-{fileId}.transcript"),
            restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
            outputTemplate: "{Message:lj}{NewLine}",
            rollingInterval: RollingInterval.Infinite)
        .CreateLogger();
    console.MarkupLineInterpolated($"Logging to [yellow]{logFilePath}[/]");

    ServiceCollection services = new();
    services.AddSingleton(console);
    services.AddScoped<Adventure, IslandAdventure>();
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog(Log.Logger, dispose: true);
    });

    // Load configuration settings and options
    IConfigurationRoot config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>(optional: true)
        .AddCommandLine(args)
        .Build();
    services.Configure<AppSettings>(config);
    services.Configure<OllamaSettings>(config.GetSection("Ollama"));
    services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppSettings>>().Value);
    services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<OllamaSettings>>().Value);

    // Configure Ollama
    OllamaSettings ollamaSettings = config.GetSection("Ollama").Get<OllamaSettings>() 
        ?? throw new InvalidOperationException("Ollama settings are not configured properly.");
    services.AddOllamaChatClient(ollamaSettings.ChatModelId, new Uri(ollamaSettings.ChatEndpoint));
    
    // Configure Semantic Kernel
    services.AddKernel();
    services.AddKeyedTransient<ChatOptions>(ChatClients.Simple, (sp, key) => new ChatOptions()
    {
        AllowMultipleToolCalls = false,
        ToolMode = ChatToolMode.None,
    });
    services.AddKeyedTransient<ChatOptions>(ChatClients.SimpleSemanticKernel, (sp, key) => new ChatOptions()
    {
        AllowMultipleToolCalls = false,
        ToolMode = ChatToolMode.None,
    });
    services.AddKeyedTransient<ChatOptions>(ChatClients.AdventureKernel, (sp, key) => new ChatOptions()
    {
        AllowMultipleToolCalls = true,
        ToolMode = ChatToolMode.Auto,
    });
    //services.AddChatClient(sp => sp.GetRequiredService<IChatClient>());    
    services.AddKeyedChatClient(ChatClients.Simple, sp =>
    {
        return sp.GetRequiredService<IChatClient>();
    });
    services.AddKeyedChatClient(ChatClients.SimpleSemanticKernel, sp =>
    {
        Kernel kernel = sp.GetRequiredService<Kernel>();
        return kernel.GetRequiredService<IChatClient>();
    });
    services.AddKeyedChatClient(ChatClients.AdventureKernel, sp =>
    {
        Kernel kernel = sp.GetRequiredService<Kernel>();
        kernel.AddAdventurePlugins(sp.GetRequiredService<Adventure>());
        return kernel.GetRequiredService<IChatClient>();
    });    

    Func<IServiceProvider, object?, IConsoleChatClient> chatClientFactory = (sp, key) =>
    {
        IChatClient chatClient = sp.GetRequiredKeyedService<IChatClient>(key);
        ChatOptions chatOptions = sp.GetRequiredKeyedService<ChatOptions>(key);
        return new ConsoleChatClient(chatClient, chatOptions, sp.GetRequiredService<IAnsiConsole>(), sp.GetRequiredService<ILogger<ConsoleChatClient>>());
    };
    services.AddKeyedTransient(ChatClients.Simple, chatClientFactory);
    services.AddKeyedTransient(ChatClients.SimpleSemanticKernel, chatClientFactory);
    services.AddKeyedTransient(ChatClients.AdventureKernel, chatClientFactory);

    // Auto-register interfaces and their implementations in this namespace using Scrutor
    services.Scan(scan => scan
        .FromAssemblyOf<Program>()
        .AddClasses(classes => classes.InNamespaceOf<Program>())
        .AddClasses(classes => classes.InNamespaces(
            typeof(SimpleChatCommand).Namespace!))
        .AsSelfWithInterfaces()
        .WithScopedLifetime()
    );

    // Register our spectre console app
    TypeRegistrar registrar = new(services);
    CommandApp<SelectCommandCommand> app = new CommandApp<SelectCommandCommand>(registrar);
    app.Configure(a =>
    {
        a.SetApplicationName("AI TableTop Game Master")
            .CaseSensitivity(CaseSensitivity.None)
            .PropagateExceptions()
            .UseAssemblyInformationalVersion()
            .ConfigureConsole(console)
            .SetApplicationCulture(CultureInfo.CurrentUICulture);

        // Add commands to the application
        a.AddCommand<SimpleChatCommand>("simple-chat")
            .WithDescription("Start a simple chat session with the AI game master.")
            .WithExample("simple-chat");
        a.AddCommand<EmptySemanticKernelChatCommand>("empty-semantic-kernel-chat")
            .WithDescription("Start a chat session with the AI game master using an empty semantic kernel chat client.")
            .WithExample("empty-semantic-kernel-chat");        
        a.AddCommand<AdventureKernelChatCommand>("adventure-kernel-chat")
            .WithDescription("Start a chat session with the AI game master using an semantic kernel and static adventure information.")
            .WithExample("adventure-kernel-chat");
    });

    // Run the application
    int result = await app.RunAsync(args);
    console.WriteLine();

    if (result != 0)
    {
        console.MarkupLine("[red]An error occurred while running the application.[/]");
        return result;
    }
    
    console.MarkupLine("[yellow]Thank you for using the AI TableTop Game Master![/]");
    return result;
} 
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
    console.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
