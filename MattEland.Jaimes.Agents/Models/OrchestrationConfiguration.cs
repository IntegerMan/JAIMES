namespace MattEland.Jaimes.Agents.Models;

public record OrchestrationConfiguration
{
    public required IDictionary<string, string> ModelServiceAssignments { get; init; }
}