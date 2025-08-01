using AiTableTopGameMaster.ConsoleShared.Clients;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.ConsoleShared.Settings;
using AiTableTopGameMaster.Core.Cores;
using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Plugins.Sourcebooks;
using AiTableTopGameMaster.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Serilog;
using Spectre.Console;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using KernelExtensions = AiTableTopGameMaster.Core.Helpers.KernelExtensions;

namespace AiTableTopGameMaster.ConsoleShared.Infrastructure;

public static class ServiceExtensions
{
    public static ServiceProvider BuildServiceProvider(IAnsiConsole console, string logFileName, string[] args)
    {
        ServiceCollection services = new();
        services.AddSingleton(console);
        services.AddJaimesAppLogging(logFileName);

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
        
        // Register Plugins
        Type[] pluginTypes = KernelExtensions.FindPluginTypesWithKernelFunctions().ToArray();
        IDictionary<string, Type> pluginTypeDictionary = KernelExtensions.BuildPluginTypeDictionary();
        Log.Debug("Found {PluginCount} plugin types with kernel functions", pluginTypeDictionary.Count);
        foreach (Type pluginType in pluginTypes)
        {
            Log.Debug("Registering plugin type: {PluginName}", pluginType.FullName);
            services.AddScoped(pluginType);
        }
        
        services.AddScoped<IEnumerable<AiCore>>(sp =>
        {
            IEnumerable<CoreInfo> infos = sp.GetServices<CoreInfo>();
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            ILogger log = loggerFactory.CreateLogger("AI Core Configuration");
            
            return infos.Select(core =>
            {
                IKernelBuilder builder = sp.GetRequiredService<IKernelBuilder>();
                
                foreach (var plugin in core.Plugins)
                {
                    if (!pluginTypeDictionary.TryGetValue(plugin, out Type? pluginType))
                    {
                        log.LogWarning("Plugin {PluginName} not found in registered plugin types", plugin);
                        continue;
                    }
                    log.LogDebug("Adding plugin {PluginName} to AI Core {CoreName}", plugin, core.Name);
                    object pluginInstance = sp.GetRequiredService(pluginType);
                    builder.Plugins.AddFromObject(pluginInstance);
                }

                Kernel kernel = builder.Build();
                
                return new AiCore(kernel, core, loggerFactory);
            });
        });
        services.AddTransient<PromptExecutionSettings>(_ => new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        });
        
        // Configure AI Cores
        if (settings.Cores.Count <= 0) throw new InvalidOperationException("No AI cores configured");
        foreach (var core in settings.Cores)
        {
            Log.Debug("Adding AI Core: {CoreName} ({CoreId})", core.Name, core.Description);
            services.AddScoped<CoreInfo>(_ => core);
        }

        // Configure application dependencies
        services.AddTransient<ConsoleChatClient>();
        services.AddSingleton<IAdventureLoader, AdventureLoader>();

        // Load adventure from JSON file
        services.AddScoped<Adventure>(sp =>
        {
            IAdventureLoader loader = sp.GetRequiredService<IAdventureLoader>();
            string adventuresPath = Path.Combine(AppContext.BaseDirectory, "adventures");

            Adventure[] adventures = loader.GetAdventuresAsync(adventuresPath).GetAwaiter().GetResult().ToArray();
            ILogger log = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Adventure Loading");

            log.LogDebug("Found {AdventureCount} adventure(s) in {Path}", adventures.Length, adventuresPath);
            if (adventures.Length == 0)
            {
                throw new InvalidOperationException($"No adventures found in {adventuresPath}");
            }

            if (adventures.Length == 1)
            {
                Adventure adventure = adventures[0];
                log.LogDebug("Only one adventure found, automatically selecting: {AdventureName}", adventure.Name);
                return adventure;
            }
            
            return console.Prompt(new SelectionPrompt<Adventure>().Title("Select an adventure:")
                .AddChoices(adventures)
                .UseConverter(a => $"{a.Name} by {a.Author}, v{a.Version}"));
        });
        services.AddScoped<Character>(sp =>
        {
            Adventure adventure = sp.GetRequiredService<Adventure>();

            Character character;
            if (adventure.Characters.Count == 1)
            {
                character = adventure.Characters[0];
                Log.Information("Only one character found, automatically selecting: {CharacterName}", character.Name);
            }
            else
            {
                character = console.Prompt(new SelectionPrompt<Character>()
                    .AddChoices(adventure.Characters)
                    .Title("Select a character:")
                    .UseConverter(c => $"{c.Name} - {c.Specialization}"));
            }
            
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
}