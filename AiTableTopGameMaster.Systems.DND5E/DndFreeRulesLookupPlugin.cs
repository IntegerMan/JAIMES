using System.ComponentModel;
using System.Text;
using AiTableTopGameMaster.Domain;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Systems.DND5E;

[Description("Contains functions for looking up rules related to Dungeons & Dragons 5th Edition (DND5E)'s free ruleset.")]
public class DndFreeRulesLookupPlugin
{
    private IKernelMemory? _memory;

    public async Task InitializeAsync(OllamaSettings settings, Action<IndexingInfo>? indexCallback = null)
    {
        _memory = new KernelMemoryBuilder()
            .WithOllamaTextGeneration(settings.ChatModelId, settings.ChatEndpoint)
            .WithOllamaTextEmbeddingGeneration(settings.EmbeddingModelId, settings.EmbeddingEndpoint)
            .WithDefaultWebScraper()
            .Build();

        await IndexDocumentAsync("https://www.dndbeyond.com/sources/dnd/br-2024", "DND5EFreeRulesTableOfContents", indexCallback);
        // TODO: Discover all links on the page and index them as well
    }

    private async Task IndexDocumentAsync(string url, string documentId, Action<IndexingInfo>? indexCallback = null)
    {
        TagCollection tags = new()
        {
            { "Source", "DND5E Free Rules" },
            { "System", "DND5E"},
            { "Url", url }
        };
        
        IndexingInfo status = new(url, documentId, IsComplete: false);
        indexCallback?.Invoke(status);
        string result = await _memory!.ImportWebPageAsync(url, documentId: documentId, tags, index: "DND5E");
        
        indexCallback?.Invoke(status with {IsComplete = true, DocumentId = result});
    }

    [KernelFunction]
    [Description("Provides the most relevant results for a rule or concept in the DND5E free ruleset.")]
    public async Task<string> RulesSearch([Description("The string to search for")] string query)
    {
        IKernelMemory memory = _memory ?? throw new InvalidOperationException("The DND5E free rules lookup plugin has not been initialized. Call InitializeAsync() before using this function.");

        SearchResult results = await memory.SearchAsync(query);
        
        if (results.NoResult)
        {
            return $"No results found for '{query}' in the DND5E free ruleset.";
        }
        
        StringBuilder sb = new();
        sb.AppendLine("Most relevant results:");
        foreach (var result in results.Results)
        {
            sb.AppendLine(result.DocumentId);
            foreach (var part in result.Partitions)
            {
                sb.AppendLine($"- {part.Text}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}