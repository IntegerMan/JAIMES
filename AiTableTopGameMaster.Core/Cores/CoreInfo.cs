namespace AiTableTopGameMaster.Core.Cores;

public record CoreInfo
{
    public required string Name { get; init; }
    public required string ModelId { get; init; }
    public string? Description { get; init; }
    public string[] Instructions { get; init; } = [];
    public bool IncludeHistory { get; init; }
    public bool IncludePlayerInput { get; init; }
    public string[] Plugins { get; init; } = [];
}