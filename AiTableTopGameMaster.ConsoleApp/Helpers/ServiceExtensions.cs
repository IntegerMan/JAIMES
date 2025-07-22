using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using AiTableTopGameMaster.ConsoleApp.Settings;
using AiTableTopGameMaster.Core;
using AiTableTopGameMaster.Core.Services;
using AiTableTopGameMaster.Domain;
using AiTableTopGameMaster.Systems.DND5E;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Serilog;
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

        // Configure Semantic Kernel
        services.AddTransient<Kernel>(sp =>
        {
            IKernelBuilder builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.ConfigureSerilogLogging(disposeLogger: false));
            builder.Services.AddSingleton(sp.GetRequiredService<IAnsiConsole>());
            builder.Services.AddSingleton<IAutoFunctionInvocationFilter, FunctionInvocationLoggingFilter>();
            return builder
                .AddOllamaChatCompletion(settings.Ollama.ChatModelId, new Uri(settings.Ollama.ChatEndpoint))
                .AddOllamaEmbeddingGenerator(settings.Ollama.EmbeddingModelId,
                    new Uri(settings.Ollama.EmbeddingEndpoint))
                .AddAdventurePlugins(sp.GetRequiredService<Adventure>())
                .AddDnd5ERulesLookup(settings.SourcebookPath, settings.Ollama,
                    status => DocumentIndexingCallback(console, status))
                .Build();
        });
        services.AddTransient<PromptExecutionSettings>(_ => new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        });

        // Configure application dependencies
        services.AddTransient<IConsoleChatClient, ConsoleChatClient>();
        services.AddSingleton<IAdventureLoader, AdventureLoader>();

        // Load adventure from JSON file
        services.AddScoped<Adventure>(sp =>
        {
            var loader = sp.GetRequiredService<IAdventureLoader>();
            var adventuresPath = Path.Combine(AppContext.BaseDirectory, "adventures");

            Adventure[] adventures = loader.GetAdventuresAsync(adventuresPath).GetAwaiter().GetResult().ToArray();

            Log.Debug("Found {AdventureCount} adventure(s) in {Path}", adventures.Length, adventuresPath);
            if (adventures.Length == 0)
            {
                throw new InvalidOperationException($"No adventures found in {adventuresPath}");
            }

            return console.Prompt(new SelectionPrompt<Adventure>().Title("Select an adventure:")
                .AddChoices(adventures)
                .UseConverter(a => $"{a.Name} by {a.Author}, v{a.Version}"));
        });
        services.AddScoped<Character>(sp =>
        {
            Adventure adventure = sp.GetRequiredService<Adventure>();
            return console.Prompt(new SelectionPrompt<Character>()
                .AddChoices(adventure.Characters)
                .Title("Select a character:")
                .UseConverter(c => $"{c.Name} - {c.Specialization}"));
        });

        return services.BuildServiceProvider();
    }

    private static void DocumentIndexingCallback(IAnsiConsole console, IndexingInfo status)
    {
        Log.Debug("Indexing {Url} as {DocumentId}: {Status}", status.Location, status.DocumentId,
            status.IsComplete ? "Complete" : "In Progress");

        console.MarkupLine(status.IsComplete
            ? $"{DisplayHelpers.ToolCallResult}Indexed {status.Location} as {status.DocumentId}[/]"
            : $"{DisplayHelpers.ToolCall}Indexing {status.Location} as {status.DocumentId}...[/]");
    }
}