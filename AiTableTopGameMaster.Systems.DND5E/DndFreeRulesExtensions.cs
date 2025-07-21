using AiTableTopGameMaster.Domain;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Systems.DND5E;

public static class DndFreeRulesExtensions
{
    public static IKernelBuilder AddDnd5ERulesLookup(this IKernelBuilder builder, OllamaSettings settings, Action<IndexingInfo>? indexingCallback)
    {
        DndFreeRulesLookupPlugin plugin = new();
        
        plugin.InitializeAsync(settings, indexingCallback).GetAwaiter().GetResult(); // Ensure the plugin is initialized before adding it
        builder.Plugins.AddFromObject(plugin);
        
        return builder;
    }
}