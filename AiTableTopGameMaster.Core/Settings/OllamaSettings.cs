namespace AiTableTopGameMaster.Core.Settings;

public class OllamaSettings
{
    public required string SystemPrompt { get; init; }
    public required string ChatModelId { get; init; }
    public required string ChatEndpoint { get; init; }
    public required string EmbeddingModelId { get; init; }
    public required string EmbeddingEndpoint { get; init; }
}
