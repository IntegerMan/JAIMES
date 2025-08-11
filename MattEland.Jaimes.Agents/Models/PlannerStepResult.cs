using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Models;

public class PlannerStepResult
{
    public required PlannerResponse Plan { get; init; }
    public required ChatHistory History { get; init; }
    public required ChatResponse Response { get; init; }
}