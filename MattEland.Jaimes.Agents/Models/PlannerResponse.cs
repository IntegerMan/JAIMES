namespace MattEland.Jaimes.Agents.Definitions;

public record PlannerResponse
{
    public required string Checks { get; init; } = "";
    public required List<string> KeyPoints { get; init; } = [];
    public required string Cautions { get; init; } = "";
}