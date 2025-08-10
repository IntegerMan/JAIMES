namespace AiTableTopGameMaster.Core.Models;

public record ModelInfo
{
    /// <summary>
    /// Unique identifier for the model. This is used for internal lookup from Cores and other services.
    /// This often will be the same as the ModelId.
    /// </summary>
    public required string Id { get; init; }
    /// <summary>
    /// The provider of the model, such as OpenAI, Azure, or a custom provider
    /// </summary>
    public required ModelProvider Provider { get; init; }
    public required ModelType Type { get; init; }
    public required string Endpoint { get; init; }
    public required string ModelId { get; init; }
    public bool SupportsTools { get; init; }
    public string? Notes { get; init; }
}