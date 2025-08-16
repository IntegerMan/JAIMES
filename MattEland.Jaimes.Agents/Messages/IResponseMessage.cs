using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public interface IResponseMessage
{
    string Response { get; init; }
    ChatHistory History { get; init; }
    string StepName { get; init; }
}