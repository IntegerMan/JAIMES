using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public record ResponseFinalizedMessage : JaimesMessage, IResponseMessage
{
    public required string Draft { get; init; }
    public required string Response { get; init; }
    public required ChatHistory History { get; init; }
    public required string StepName { get; init; }
    public required string ServiceId { get; init; }
}