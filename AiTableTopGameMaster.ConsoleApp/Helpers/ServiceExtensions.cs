using AiTableTopGameMaster.Adventures.IslandAdventureDemo;
using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Settings;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class ServiceExtensions
{
    public static ServiceProvider BuildServiceProvider(IAnsiConsole console, string[] args)
    {
        ServiceCollection services = new();
        services.AddSingleton(console);
        services.AddAigmAppLogging();

        // Load configuration settings and options
        AppSettings settings = services.RegisterConfigurationAndSettings(args);

        // Configure Ollama (In the future we might want to support other providers based on root config settings)
        services.AddOllamaChatClient(settings.Ollama.ChatModelId, new Uri(settings.Ollama.ChatEndpoint));
    
        // Configure Semantic Kernel
        services.AddKernel();
        services.AddTransient<ChatOptions>(_ => new ChatOptions()
        {
            AllowMultipleToolCalls = false,
            ToolMode = ChatToolMode.None,
        });
    
        // Configure application dependencies
        services.AddTransient<IConsoleChatClient, ConsoleChatClient>();
        services.AddScoped<Adventure, IslandAdventure>(); // It'd be nice to let the user choose the adventure type
        
        return services.BuildServiceProvider();
    }
}