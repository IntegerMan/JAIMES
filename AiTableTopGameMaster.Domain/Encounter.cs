namespace AiTableTopGameMaster.Domain;

public record Encounter
{
    public required string Id { get; init; }
    public required string Description { get; init; }
}