using MattEland.Jaimes.Agents.Models;

namespace MattEland.Jaimes.Agents.Messages;

public interface IResponseMessage : IHasHistory
{
    string Response { get; init; }
    string StepName { get; init; }
    OrchestrationConfiguration Configuration { get; init; }
}