using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Systems.DND5E;

public static class DndFreeRulesExtensions
{
    public static IKernelBuilder AddDnd5ERulesLookup(this IKernelBuilder builder, ILoggerFactory logger, OllamaSettings settings)
    {
        DndFreeRulesLookupPlugin plugin = new(logger);
        
        plugin.InitializeAsync(settings).GetAwaiter().GetResult(); // Ensure the plugin is initialized before adding it
        builder.Plugins.AddFromObject(plugin);
        
        return builder;
    }
}