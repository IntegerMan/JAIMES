namespace MattEland.Jaimes.Agents.Planner;

public record PlannerResponse
{
    public required string Checks { get; init; } = "";
    public required string KeyPoints { get; init; } = "";
    public required string Cautions { get; init; } = "";
}