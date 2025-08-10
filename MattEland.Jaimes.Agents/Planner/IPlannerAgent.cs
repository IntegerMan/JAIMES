namespace MattEland.Jaimes.Agents;

public interface IPlannerAgent
{
    Task<PlannerResponse> GenerateAsync();
}