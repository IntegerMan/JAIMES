using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public record ResponseComposedMessage : JaimesMessage, IResponseMessage
{
    public required string Response { get; init; }
    public required ChatHistory History { get; init; }
    public required string StepName { get; init; }
    public required string ServiceId { get; init; }
}