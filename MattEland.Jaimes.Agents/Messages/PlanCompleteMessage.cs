using MattEland.Jaimes.Agents.Models;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public record PlanCompleteMessage
{
    public required ChatHistory History { get; init; }
    public required PlannerResponse Plan { get; init; }
    public required ChatResponse Response { get; init; }
}