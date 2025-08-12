using MattEland.Jaimes.Core.Domain;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public record ConversationMessage
{
    public required ChatHistory History { get; init; }
    public required Adventure Adventure { get; init; }
    public required Character Character { get; init; }
}
