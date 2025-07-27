namespace AiTableTopGameMaster.Core.Domain;

public record Location
{
    public required string Name { get; init; }
    public required string Description { get; init; }
}