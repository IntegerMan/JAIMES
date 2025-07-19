using AiTableTopGameMaster.Adventures.IslandAdventureDemo;
using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using AiTableTopGameMaster.ConsoleApp.Settings;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
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
    services.AddTransient<ChatOptions>(_ => new ChatOptions()
    {
        AllowMultipleToolCalls = false,
        ToolMode = ChatToolMode.None,
    });
    
    services.AddTransient<IConsoleChatClient, ConsoleChatClient>();

    ServiceProvider sp = services.BuildServiceProvider();

    Adventure adventure = sp.GetRequiredService<Adventure>();
    ICollection<ChatMessage> history = adventure.GenerateInitialHistory();
    IConsoleChatClient client =  sp.GetRequiredService<IConsoleChatClient>();
    
    await client.ChatIndefinitelyAsync(history);
    
    console.WriteLine();
    return 0;
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
