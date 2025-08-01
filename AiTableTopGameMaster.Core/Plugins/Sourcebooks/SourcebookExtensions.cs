using AiTableTopGameMaster.Core.Settings;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core.Plugins.Sourcebooks;

public static class SourcebookExtensions
{
    public static IKernelBuilder AddSourcebooks(this IKernelBuilder builder, string system, string sourcebookPath, OllamaSettings settings, Action<IndexingInfo>? indexingCallback)
    {
        SourcebookLookupPlugin plugin = new(sourcebookPath, system);
        
        plugin.InitializeAsync(settings, indexingCallback).GetAwaiter().GetResult(); // Ensure the plugin is initialized before adding it
        builder.Plugins.AddFromObject(plugin);
        
        return builder;
    }
}