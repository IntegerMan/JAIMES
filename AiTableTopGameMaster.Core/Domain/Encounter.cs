namespace AiTableTopGameMaster.Core.Domain;

public record Encounter
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}