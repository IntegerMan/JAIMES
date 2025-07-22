namespace AiTableTopGameMaster.Domain;

public class Character
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Level { get; init; }
    public required string Class { get; init; }
    public bool IsPlayerCharacter { get; init; }
}