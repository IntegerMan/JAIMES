using MattEland.Jaimes.Core.Models;

namespace AiTableTopGameMaster.ConsoleApp;

public record ModelConfiguration
{
    public required string ModelId { get; init; }
    public required string ProviderId { get; init; }
    public required ModelType Usage { get; init; }
    public bool SupportsTools { get; init; } = false;
}