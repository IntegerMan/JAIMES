using JetBrains.Annotations;

namespace AiTableTopGameMaster.ConsoleApp;

[UsedImplicitly]
public class AppSettings
{
    public required string SourcebookPath { get; init; }
    public required string EmbeddingModelId { get; init; }
}