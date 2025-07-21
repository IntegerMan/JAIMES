using AiTableTopGameMaster.Domain;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Systems.DND5E;

public static class DndFreeRulesExtensions
{
    public static IKernelBuilder AddDnd5ERulesLookup(this IKernelBuilder builder, string sourcebookPath, OllamaSettings settings, Action<IndexingInfo>? indexingCallback)
    {
        DndFreeRulesLookupPlugin plugin = new(sourcebookPath);
        
        plugin.InitializeAsync(settings, indexingCallback).GetAwaiter().GetResult(); // Ensure the plugin is initialized before adding it
        builder.Plugins.AddFromObject(plugin);
        
        return builder;
    }
}