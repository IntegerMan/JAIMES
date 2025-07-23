using System.ComponentModel;
using System.Text;
using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core.Plugins.Sourcebooks;

[Description("Contains functions for looking up rules related to Dungeons & Dragons 5th Edition (DND5E)'s free ruleset.")]
public class SourcebookLookupPlugin(string sourceDirectory, string system)
{
    private IKernelMemory? _memory;

    public async Task InitializeAsync(OllamaSettings settings, Action<IndexingInfo>? indexCallback = null)
    {
        _memory = new KernelMemoryBuilder()
            .WithOllamaTextGeneration(settings.ChatModelId, settings.ChatEndpoint)
            .WithOllamaTextEmbeddingGeneration(settings.EmbeddingModelId, settings.EmbeddingEndpoint)
            .Build();

        // List all PDF files in the source directory
        string[] pdfFiles = Directory.GetFiles(sourceDirectory, "*.pdf", SearchOption.AllDirectories);
        if (pdfFiles.Length == 0)
        {
            throw new InvalidOperationException($"No PDF files found in directory {sourceDirectory}");
        }
        
        // Index each PDF file
        foreach (string pdfFile in pdfFiles)
        {
            string documentId = Path.GetFileNameWithoutExtension(pdfFile).Replace(" ", "_");
            await IndexDocumentAsync(pdfFile, documentId, indexCallback);
        }
    }

    private async Task IndexDocumentAsync(string filePath, string documentId, Action<IndexingInfo>? indexCallback = null)
    {
        TagCollection tags = new()
        {
            { "RulesSystem", system},
            { "Location", filePath }
        };
        
        IndexingInfo status = new(filePath, documentId, IsComplete: false);
        indexCallback?.Invoke(status);
        string result = await _memory!.ImportDocumentAsync(filePath, documentId, tags);
        
        indexCallback?.Invoke(status with {IsComplete = true, DocumentId = result});
    }

    [KernelFunction, UsedImplicitly]
    [Description("Retrieves possibly helpful passages from the DND5E free ruleset based on a search query.")]
    public async Task<string> RulesSearch([Description("A topic or question to search for")] string query)
    {
        IKernelMemory memory = _memory ?? throw new InvalidOperationException("The DND5E free rules lookup plugin has not been initialized. Call InitializeAsync() before using this function.");

        SearchResult results = await memory.SearchAsync(query);
        if (results.NoResult)
        {
            return $"No results found for '{query}' in the DND5E free ruleset.";
        }
        
        // Find the top 5 most relevant Partitions
        results.Results.SelectMany(p => p.Partitions).OrderByDescending(p => p.Relevance).Take(5);
        
        
        StringBuilder sb = new();
        sb.AppendLine("Here are a few of the most relevant passages from the rules:");
        foreach (Citation.Partition result in results.Results
                     .SelectMany(p => p.Partitions)
                     .OrderByDescending(p => p.Relevance)
                     .Take(5))
        {
            sb.AppendLine(result.Text);
        }

        sb.AppendLine("Keep in mind that these are just the most relevant results. Use these results to respond to the player, or make up a response that feels appropriate.");
        
        return sb.ToString();
    }
    
    [KernelFunction, UsedImplicitly]
    [Description("Asks a rules expert about a specific rule or concept in the ruleset.")]
    public async Task<string> ConsultRules([Description("A question for the rulebook")] string query)
    {
        IKernelMemory memory = _memory ?? throw new InvalidOperationException("The plugin has not been initialized. Call InitializeAsync() before using this function.");

        MemoryAnswer results = await memory.AskAsync(query);
        if (results.NoResult)
        {
            return $"No results found for '{query}' in the available rules: {results.NoResultReason}";
        }

        return results.Result;
    }
}