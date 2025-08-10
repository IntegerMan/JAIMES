using AiTableTopGameMaster.Core;
using AiTableTopGameMaster.Core.Models;
using JetBrains.Annotations;

namespace AiTableTopGameMaster.ConsoleApp;

[UsedImplicitly]
public class AppSettings : ISettingsRoot
{
    public required string SourcebookPath { get; init; }
    public required string EmbeddingModelId { get; init; }
    public AzureOpenAIModelSettings AzureOpenAI { get; init; } = new();
    
    public required string EvaluationModelId { get; init; }
    public required string EvaluationStoragePath { get; init; }
    public int EvaluationIterations { get; init; } = 1;
    public string[] ModelIdsToEvaluate { get; init; } = [];
}