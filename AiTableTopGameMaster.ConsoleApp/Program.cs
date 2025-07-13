using AiTableTopGameMaster.ConsoleApp.Settings;
using AiTableTopGameMaster.ConsoleApp.Workers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;

IAnsiConsole console = AnsiConsole.Console;
IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.Sources.Clear();
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(optional: true)
            .AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(console);
        services.AddLogging();

        // Add options pattern for AppSettings and OllamaSettings
        services.Configure<AppSettings>(context.Configuration);
        services.Configure<OllamaSettings>(context.Configuration.GetSection("Ollama"));
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
        
        services.AddHostedService<ApplicationWorker>();
    });

IHost host = builder.Build();
await host.StartAsync();