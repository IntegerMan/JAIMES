using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Core.Domain;

public class ConversationContext
{
    public required ChatHistory History { get; init; }
    public required Adventure Adventure { get; init; }
    public required Character Character { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }
}