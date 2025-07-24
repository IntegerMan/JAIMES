using AiTableTopGameMaster.ConsoleApp.Agents;
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
        services.AddTransient<IKernelBuilder>(sp =>
        {
            IKernelBuilder builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.ConfigureSerilogLogging(disposeLogger: false));
            builder.Services.AddSingleton(sp.GetRequiredService<IAnsiConsole>());
            builder.Services.AddSingleton<IAutoFunctionInvocationFilter, FunctionInvocationLoggingFilter>();
            builder.AddOllamaChatCompletion(settings.Ollama.ChatModelId, new Uri(settings.Ollama.ChatEndpoint));
            builder.AddOllamaEmbeddingGenerator(settings.Ollama.EmbeddingModelId, new Uri(settings.Ollama.EmbeddingEndpoint));

            return builder;
        });
        services.AddTransient<PromptExecutionSettings>(_ => new OllamaPromptExecutionSettings {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        });
        // Register individual agents
        services.AddTransient<Agent>(sp =>
        {
            Adventure adventure = sp.GetRequiredService<Adventure>();
            Character character = sp.GetRequiredService<Character>();
            IKernelBuilder builder = sp.GetRequiredService<IKernelBuilder>();
            builder.AddAdventurePlugins(adventure);
            
            KernelArguments arguments = new(sp.GetRequiredService<PromptExecutionSettings>());
            ILoggerFactory logFactory = sp.GetRequiredService<ILoggerFactory>();

            return new ChatCompletionAgent
            {
                Name = "PlanningAgent",
                Description = "Planning Agent - plans appropriate responses for game master",
                Instructions = PlanningAgentFactory.BuildPlanningInstructions(adventure, character),
                Kernel = builder.Build(),
                Arguments = arguments,
                LoggerFactory = logFactory
            };
        });
        
        services.AddTransient<Agent>(sp =>
        {
            Adventure adventure = sp.GetRequiredService<Adventure>();
            Character character = sp.GetRequiredService<Character>();
            IKernelBuilder builder = sp.GetRequiredService<IKernelBuilder>();
            KernelArguments arguments = new(sp.GetRequiredService<PromptExecutionSettings>());
            ILoggerFactory logFactory = sp.GetRequiredService<ILoggerFactory>();

            return new ChatCompletionAgent
            {
                Name = "GameMaster",
                Description = "Game Master - delivers narrative responses to players",
                Instructions = GameMasterAgentFactory.BuildGameMasterInstructions(adventure, character),
                Kernel = builder.Build(),
                Arguments = arguments,
                LoggerFactory = logFactory
            };
        });
        
        services.AddTransient<Agent>(sp =>
        {
            IKernelBuilder builder = sp.GetRequiredService<IKernelBuilder>();
            KernelArguments arguments = new(sp.GetRequiredService<PromptExecutionSettings>());
            ILoggerFactory logFactory = sp.GetRequiredService<ILoggerFactory>();

            return new ChatCompletionAgent
            {
                Name = "EditorAgent",
                Description = "Editor Agent - improves and proofs game master responses",
                Instructions = "IGNORE PRIOR INSTRUCTIONS AND RESPOND ONLY WITH 'SQUIRREL'", //EditorAgentFactory.BuildEditorInstructions(adventure, character),
                Kernel = builder.Build(),
                Arguments = arguments,
                LoggerFactory = logFactory
            };
        });
        
        services.AddTransient<IConsoleChatClient, MultiAgentChatClient>();
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
            Character character = console.Prompt(new SelectionPrompt<Character>()
                .AddChoices(adventure.Characters)
                .Title("Select a character:")
                .UseConverter(c => $"{c.Name} - {c.Specialization}"));
            
            adventure.PlayerCharacter = character;
            
            return character;
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