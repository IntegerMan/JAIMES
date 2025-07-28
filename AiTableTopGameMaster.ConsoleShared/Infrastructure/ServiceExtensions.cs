using System.Text.Json;
using System.Text.Json.Serialization;
using AiTableTopGameMaster.ConsoleShared.Clients;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.ConsoleShared.Settings;
using AiTableTopGameMaster.Core;
using AiTableTopGameMaster.Core.Cores;
using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Models;
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
    public static ServiceProvider BuildServiceProvider<TSettings>(IAnsiConsole console, string logFileName, string[] args) where TSettings : class, ISettingsRoot
    {
        ServiceCollection services = new();
        services.AddSingleton(console);
        services.AddJaimesAppLogging(logFileName);
        services.AddSingleton<ModelFactory>();
        services.AddSingleton<IPromptsService, PromptsService>();

        // Load configuration settings and options
        services.RegisterConfigurationAndSettings<TSettings>(args);
        
        // Configure Semantic Kernel
        services.AddTransient<IKernelBuilder>(sp =>
        {
            IKernelBuilder builder = Kernel.CreateBuilder();
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.ConfigureSerilogLogging(disposeLogger: false));
            builder.Services.AddSingleton(sp.GetRequiredService<IAnsiConsole>());
            builder.Services.AddSingleton<IAutoFunctionInvocationFilter, FunctionInvocationLoggingFilter>();
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
        services.AddTransient<PromptExecutionSettings>(_ => new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        });
        services.AddScoped<JsonSerializerOptions>(_ =>
        {
            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        });
        services.AddScoped<IEnumerable<ModelInfo>>(sp =>
        {
            string path = Path.Combine(Environment.CurrentDirectory, "ai", "models.json");
            Log.Debug("Reading Model Configurations from {Filename}", path);
            JsonSerializerOptions options = sp.GetRequiredService<JsonSerializerOptions>();
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<List<ModelInfo>>(stream, options) ?? [];
        });
        services.AddScoped<IEnumerable<CoreInfo>>(sp =>
        {
            string path = Path.Combine(Environment.CurrentDirectory, "ai", "cores.json");
            Log.Debug("Reading AI Cores from {Filename}", path);
            JsonSerializerOptions options = sp.GetRequiredService<JsonSerializerOptions>();
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<List<CoreInfo>>(stream, options) ?? [];
        });
        services.AddSingleton<StandardPrompts>(sp =>
        {
            string path = Path.Combine(Environment.CurrentDirectory, "ai", "prompts.json");
            Log.Debug("Reading Standard Prompts from {Filename}", path);
            JsonSerializerOptions options = sp.GetRequiredService<JsonSerializerOptions>();
            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<StandardPrompts>(stream, options) ?? new StandardPrompts();
        });
        services.AddSingleton<CoreFactory>();
        services.AddScoped<IEnumerable<AiCore>>(sp =>
        {
            CoreFactory factory = sp.GetRequiredService<CoreFactory>();
            CoreInfo[] cores = sp.GetServices<CoreInfo>().ToArray();
            return cores.Select(c => factory.CreateCore(c));
        });

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