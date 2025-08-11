using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Planner;

public interface IPlannerAgent : IJaimesAgent<PlannerResponse>
{
    Task<(PlannerResponse, ChatHistory)> GenerateAsync(ChatHistory history);
}