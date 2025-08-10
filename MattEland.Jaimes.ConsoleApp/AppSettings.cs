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
}