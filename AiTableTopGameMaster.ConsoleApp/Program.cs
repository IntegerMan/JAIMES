using System.Globalization;
using AiTableTopGameMaster.ConsoleApp.Commands;
using AiTableTopGameMaster.ConsoleApp.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;
using Spectre.Console.Cli;

IAnsiConsole console = AnsiConsole.Console;

try
{
    console.Write(new FigletText("AI Game Master")
        .Justify(Justify.Left)
        .Color(Color.Green));
    console.MarkupLine("By [yellow]Matt Eland[/] - [blue]https://matteland.dev[/]");

    ServiceCollection services = new();

    services.AddSingleton(console);
    services.AddLogging();

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

    services.AddChatClient(sp =>
    {
        OllamaSettings ollamaSettings = sp.GetRequiredService<OllamaSettings>();
        return new OllamaChatClient(ollamaSettings.ChatEndpoint, ollamaSettings.ChatModelId)
            .AsBuilder()
            .UseLogging(sp.GetRequiredService<ILoggerFactory>())
            .UseFunctionInvocation()
            .Build();
    });

    TypeRegistrar registrar = new(services);
    CommandApp<DefaultCommand> app = new CommandApp<DefaultCommand>(registrar);
    app.Configure(a =>
    {
        a.SetApplicationName("AI TableTop Game Master")
            .CaseSensitivity(CaseSensitivity.None)
            .PropagateExceptions()
            .UseAssemblyInformationalVersion()
            .ConfigureConsole(console)
            .SetApplicationCulture(CultureInfo.CurrentUICulture);
    });

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
    console.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}