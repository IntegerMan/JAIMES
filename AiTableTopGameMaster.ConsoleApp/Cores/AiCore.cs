using AiTableTopGameMaster.ConsoleApp.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.ConsoleApp.Cores;

public class AiCore(Kernel kernel, CoreInfo info, ILoggerFactory loggerFactory)
{
    private readonly ILogger<AiCore> _log = loggerFactory.CreateLogger<AiCore>();
    public string Name => info.Name;
    
    public override string ToString() => $"AI Core {Name}";
    
    public async IAsyncEnumerable<string> ChatAsync(string message, ChatHistory transcript)
    {
        _log.LogDebug("{CoreName}: Starting chat with message: {Message}", Name, message);
        
        IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        ChatHistory history = BuildChatHistory(message, transcript);
        history.LogHistory();

        PromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        _log.LogDebug("{CoreName}: Sending message to chat service", Name);
        foreach (var result in await chatService.GetChatMessageContentsAsync(history, settings, kernel))
        {
            string? content = result.Content;
            _log.LogDebug("{CoreName}: {Content}", Name, content);
            if (string.IsNullOrWhiteSpace(content)) continue;
            
            yield return content;
        }
    }

    private ChatHistory BuildChatHistory(string message, ChatHistory transcript)
    {
        ChatHistory history = new(); 
        history.AddSystemMessage(info.Instructions);

        ChatMessageContent? lastUserMessage = transcript.LastOrDefault(t => t.Role == AuthorRole.User);
        
        if (info.IncludeHistory && transcript.Count > 0)
        {
            _log.LogDebug("{CoreName}: Including previous chat history", Name);
            history.AddSystemMessage("Here is the previous session history:");
            foreach (var chatMessage in transcript)
            {
                // If we are including player input and the last user message is the same as the current one, skip it so we don't repeat it
                if (chatMessage == lastUserMessage && info.IncludePlayerInput) continue;
                
                // Only include user and assistant messages in the history. We don't need tool calls / results / other system messages
                if (chatMessage.Role == AuthorRole.User)
                {
                    history.AddUserMessage(chatMessage.Content!);
                }
                else if (chatMessage.Role == AuthorRole.Assistant)
                {
                    history.AddAssistantMessage(chatMessage.Content!);
                }
            }
        }
        
        if (info.IncludePlayerInput && lastUserMessage is not null)
        {
            history.AddUserMessage($"The player just said: {lastUserMessage.Content}");
        }
        
        history.AddUserMessage(message);

        return history;
    }
}