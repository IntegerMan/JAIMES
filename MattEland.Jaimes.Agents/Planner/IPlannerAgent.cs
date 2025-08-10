namespace MattEland.Jaimes.Agents;

public interface IPlannerAgent : IJaimesAgent<PlannerResponse>
{
    Task<PlannerResponse> GenerateAsync();
}