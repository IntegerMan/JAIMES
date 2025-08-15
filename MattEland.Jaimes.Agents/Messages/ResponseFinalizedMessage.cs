using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public record ResponseFinalizedMessage
{
    public required string Draft { get; init; }
    public required string Response { get; init; }
    public required ChatHistory History { get; init; }
}