namespace MattEland.Jaimes.Core.Models;

public record ModelProvider
{
    public required string ProviderId { get; init; }
    public required ModelProviderType Type { get; init; }
    public string? Url { get; init; }
    public string? ApiKey { get; init; }
}