namespace MattEland.Jaimes.Agents.Planner;

public interface IPlannerAgent : IJaimesAgent<PlannerResponse>
{
    Task<PlannerResponse> GenerateAsync();
}