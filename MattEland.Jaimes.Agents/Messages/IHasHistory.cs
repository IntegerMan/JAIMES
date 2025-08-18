using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public interface IHasHistory
{
    ChatHistory History { get; init; }
}