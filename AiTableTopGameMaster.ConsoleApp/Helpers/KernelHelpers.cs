using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class KernelHelpers
{
    public static ChatHistory ToChatHistory(this IEnumerable<ChatMessage> messages)
    {
        ChatHistory history = new();
        foreach (ChatMessage message in messages)
        {
            if (message.Role == ChatRole.User)
            {
                history.AddUserMessage(message.Text);
            }
            else if (message.Role == ChatRole.Assistant || message.Role == ChatRole.Tool)
            {
                history.AddAssistantMessage(message.Text);
            }            
            else if (message.Role == ChatRole.System)
            {
                history.AddSystemMessage(message.Text);
            }
            else
            {
                throw new ArgumentException($"Unknown chat role: {message.Role}");
            }
        }
        return history;
    }
}