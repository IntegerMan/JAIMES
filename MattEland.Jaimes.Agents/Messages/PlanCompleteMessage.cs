using MattEland.Jaimes.Agents.Models;
using Microsoft.Extensions.AI;

namespace MattEland.Jaimes.Agents.Messages;

public class PlanCompleteMessage(ConversationMessage conversation) : ConversationMessage(conversation)
{
    public required PlannerResponse Plan { get; init; }
    public required ChatResponse Response { get; init; }
}