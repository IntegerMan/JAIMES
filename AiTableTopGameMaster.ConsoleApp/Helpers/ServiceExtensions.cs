using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using AiTableTopGameMaster.ConsoleApp.Settings;
using AiTableTopGameMaster.Core;
using AiTableTopGameMaster.Core.Plugins.Sourcebooks;
using AiTableTopGameMaster.Core.Services;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.Ollama;
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
            Adventure adventure = sp.GetRequiredService<Adventure>();
            IKernelBuilder builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.ConfigureSerilogLogging(disposeLogger: false));
            builder.Services.AddSingleton(sp.GetRequiredService<IAnsiConsole>());
            builder.Services.AddSingleton<IAutoFunctionInvocationFilter, FunctionInvocationLoggingFilter>();
            return builder
                .AddOllamaChatCompletion(settings.Ollama.ChatModelId, new Uri(settings.Ollama.ChatEndpoint))
                .AddOllamaEmbeddingGenerator(settings.Ollama.EmbeddingModelId,
                    new Uri(settings.Ollama.EmbeddingEndpoint))
                .AddAdventurePlugins(sp.GetRequiredService<Adventure>())
                .AddSourcebooks(adventure.Ruleset, settings.SourcebookPath, settings.Ollama,
                    status => DocumentIndexingCallback(console, status))
                .Build();
        });
        services.AddTransient<PromptExecutionSettings>(_ => new OllamaPromptExecutionSettings {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        });
        services.AddTransient<Agent>(sp =>
        {
            Adventure adventure = sp.GetRequiredService<Adventure>();
            Character character = sp.GetRequiredService<Character>();
            Kernel kernel = sp.GetRequiredService<Kernel>();
            
            // Build the system instructions combining adventure context
            string systemInstructions = BuildSystemInstructions(adventure, character);
            
            return new ChatCompletionAgent
            {
                Name = "GameMaster",
                Description = $"Game Master for {adventure.Name} - {adventure.Ruleset} adventure",
                Instructions = systemInstructions,
                Kernel = kernel,
                Arguments = new KernelArguments(sp.GetRequiredService<PromptExecutionSettings>())
            };
        });
        
        // Register output review agent
        services.AddTransient<IOutputReviewer>(sp =>
        {
            Kernel kernel = sp.GetRequiredService<Kernel>();
            ILogger<OutputReviewAgent> logger = sp.GetRequiredService<ILogger<OutputReviewAgent>>();
            return new OutputReviewAgent(kernel, logger);
        });
        
        // EXTENSION POINT: Future multi-agent support could register additional agents here
        // For example:
        // services.AddTransient<NPCAgent>(sp => CreateNPCAgent(sp, "Merchant"));
        // services.AddTransient<WorldAgent>(sp => CreateWorldAgent(sp));
        
        services.AddTransient<IConsoleChatClient, ConsoleChatClient>();
        services.AddSingleton<IAdventureLoader, AdventureLoader>();

        // Load adventure from JSON file
        services.AddScoped<Adventure>(sp =>
        {
            IAdventureLoader loader = sp.GetRequiredService<IAdventureLoader>();
            string adventuresPath = Path.Combine(AppContext.BaseDirectory, "adventures");

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
    
    /// <summary>
    /// Builds comprehensive system instructions for the Game Master agent.
    /// Combines adventure context, character information, and game rules.
    /// </summary>
    private static string BuildSystemInstructions(Adventure adventure, Character playerCharacter)
    {
        return $"""
            {adventure.GameMasterSystemPrompt}
            
            ADVENTURE CONTEXT:
            - Adventure: {adventure.Name} by {adventure.Author}
            - Ruleset: {adventure.Ruleset}
            - Backstory: {adventure.Backstory}
            - Setting: {adventure.SettingDescription}
            
            PLAYER CHARACTER:
            - Name: {playerCharacter.Name}
            - Class/Specialization: {playerCharacter.Specialization}
            - You can check their character sheet via function calls as needed.
            
            NARRATIVE STRUCTURE:
            {adventure.NarrativeStructure}
            
            GAME MASTER NOTES:
            {adventure.GameMasterNotes}
            
            LOCATIONS OVERVIEW:
            {adventure.LocationsOverview}
            
            ENCOUNTERS OVERVIEW:
            {adventure.EncountersOverview}
            
            Remember: You have access to various functions to look up character information, 
            location details, encounter specifics, and sourcebook references. Use these tools 
            to provide rich, accurate gameplay experiences.
            """;
    }
}