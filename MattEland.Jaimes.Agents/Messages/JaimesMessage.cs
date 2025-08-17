using MattEland.Jaimes.Agents.Models;

namespace MattEland.Jaimes.Agents.Messages;

public abstract record JaimesMessage
{
    public required OrchestrationConfiguration Configuration { get; init; }
}