using JetBrains.Annotations;
using MattEland.Jaimes.Core.Models;

namespace AiTableTopGameMaster.ConsoleApp;

[UsedImplicitly]
public class AppSettings
{
    public required string SourcebookPath { get; init; }
    public required string EmbeddingModelId { get; init; }
    public required IDictionary<string, string> ModelServiceAssignments { get; init; }
    public required string EvaluationStoragePath { get; init; }
    public int EvaluationIterations { get; init; } = 1;
    public string[] ModelIdsToEvaluate { get; init; } = [];
    
    public List<ModelConfiguration> ModelConfigurations { get; init; } = [];
    public List<ModelProvider> ModelProviders { get; init; } = [];
}