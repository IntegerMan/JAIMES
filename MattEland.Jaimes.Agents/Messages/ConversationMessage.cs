using MattEland.Jaimes.Core.Domain;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public class ConversationMessage(
    ChatHistory history,
    Adventure adventure,
    Character character,
    IServiceProvider serviceProvider)
{
    public ConversationMessage(ConversationMessage message) : this(message.History, message.Adventure, message.Character, message.ServiceProvider)
    {
    }

    public ChatHistory History { get; init; } = history;
    public Adventure Adventure { get; init; } = adventure;
    public Character Character { get; init; } = character;
    public IServiceProvider ServiceProvider { get; init; } = serviceProvider;
}
