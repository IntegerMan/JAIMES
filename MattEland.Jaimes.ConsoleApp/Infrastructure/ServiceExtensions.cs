using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.ConsoleApp.Menus;
using MattEland.Jaimes.Core.Cores;
using MattEland.Jaimes.Core.Domain;
using MattEland.Jaimes.Core.Evaluation;
using MattEland.Jaimes.Core.Models;
using MattEland.Jaimes.Core.Plugins.Sourcebooks;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Serilog;
using Spectre.Console;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using KernelExtensions = MattEland.Jaimes.Core.Helpers.KernelExtensions;
#pragma warning disable SKEXP0001

namespace AiTableTopGameMaster.ConsoleApp.Infrastructure;

public static class ServiceExtensions
{
    public static IServiceProvider BuildServiceProvider(IAnsiConsole console, string logFileName, string[] args)
    {
        ServiceCollection services = new();
        services.AddSingleton(console);
        services.AddJaimesAppLogging(logFileName);
        services.AddSingleton<IPromptsService, PromptsService>();
        services.AddSingleton<IEventsService, EventsService>();
        services.AddTransient<IConversationContextService, ConversationContextService>();

        // Load configuration settings and options
        services.RegisterConfigurationAndSettings(args);
        
        // Configure Semantic Kernel
        services.AddTransient<IKernelBuilder>(sp =>
        {
            AppSettings settings = sp.GetRequiredService<AppSettings>();
            
            IKernelBuilder builder = Kernel.CreateBuilder();
            builder.Services.AddSingleton<AppSettings>(_ => sp.GetRequiredService<AppSettings>());
            builder.Services.AddSingleton<ILoggerFactory>(_ => sp.GetRequiredService<ILoggerFactory>());
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.ConfigureSerilogLogging(disposeLogger: false));
            builder.Services.AddSingleton(sp.GetRequiredService<IAnsiConsole>());
            builder.Services.AddSingleton<IAutoFunctionInvocationFilter, FunctionInvocationLoggingFilter>();
            builder.Services.AddChatClient(sp2 =>
            {
                IChatCompletionService chatService = sp2.GetRequiredService<IChatCompletionService>();
                return chatService.AsChatClient();
            });
            builder.Services.AddKeyedSingleton(serviceKey: "ModelServiceAssignments", settings.ModelServiceAssignments);
            
            IDictionary<string, ModelProvider> modelProviders = 
                settings.ModelProviders.ToFrozenDictionary(
                    p => p.ProviderId, 
                    p => p, 
                    StringComparer.OrdinalIgnoreCase);

            foreach (var model in settings.ModelConfigurations)
            {
                builder.Services.AddSingleton(model);
                
                string serviceId = $"{model.ProviderId}__{model.ModelId}";
                
                ModelProvider provider = modelProviders[model.ProviderId];
                RegisterChatCompletion(provider, builder, model, serviceId);

                // The evaluation client comes out as the default service, so it will be the one model we don't register with a serviceId.
                if (serviceId == settings.ModelServiceAssignments["Evaluator"])
                {
                    RegisterChatCompletion(provider, builder, model, null);
                }
            }
            
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
        
        // Add an IChatClient for evaluation
        /*
        services.AddKeyedSingleton<IChatClient>("Evaluation", (sp, key) =>
        {
            AppSettings settings = sp.GetRequiredService<AppSettings>();
            IModelFactory modelFactory = sp.GetRequiredService<IModelFactory>();
            return modelFactory.CreateChatClient(settings.EvaluationModelId);
        });
        */
        
        // Automatic registration of types by conventions
        services.Scan(scan =>
        {
            services.AddSingleton<EvaluationManager>();
            
            // Find all IMainMenuChoice implementations and register them
            scan.FromEntryAssembly()
                .AddClasses(c => c.AssignableTo<IMenuChoice>())
                .AsImplementedInterfaces()
                .WithTransientLifetime();
            
            // Register all EvaluationScenario implementations
            scan.FromEntryAssembly()
                .AddClasses(c => c.AssignableTo<EvaluationScenario>())
                .As<EvaluationScenario>()
                .WithTransientLifetime();
        });

        return services.BuildServiceProvider();
    }

    private static void RegisterChatCompletion(ModelProvider provider, IKernelBuilder builder, ModelConfiguration model, string? serviceId)
    {
        switch (provider.Type)
        {
            case ModelProviderType.AzureOpenAI:
                string azKey = provider.ApiKey ?? throw new InvalidOperationException("Azure OpenAI key is not configured.");
                if (string.IsNullOrWhiteSpace(provider.Url))
                {
                    throw new InvalidOperationException("Azure OpenAI URL is required.");
                }
                builder.AddAzureOpenAIChatCompletion(model.ModelId, provider.Url, azKey, serviceId: serviceId);
                break;
            case ModelProviderType.OpenAI:
                throw new NotImplementedException("OpenAI provider is not yet implemented in this version.");
                break;
            case ModelProviderType.Ollama:
                // HACK: A temporary workaround for Ollama's signatures not supporting ResponseFormat in the SDK.
                // See https://github.com/microsoft/semantic-kernel/issues/9919
                string url = provider.Url ?? "http://localhost:11434";
                if (url.EndsWith('/'))
                {
                    url = url.TrimEnd('/');
                }
                builder.AddOpenAIChatCompletion(model.ModelId, new Uri($"{url}/v1"), apiKey: "ollama", serviceId: serviceId);
                break;
            default:
                throw new NotSupportedException($"Model provider Type '{provider.Type}' is not supported.");
        }
        
        Log.Information("Registered chat completion for model {ModelId} with provider {Provider} (Service ID: {ServiceId})",
            model.ModelId, provider.Type, serviceId);
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