using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public record ResponseComposedMessage
{
    public required string Response { get; init; }
    public required ChatHistory History { get; init; }
}