using System.Text;
using AiTableTopGameMaster.Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace AiTableTopGameMaster.Core.Cores;

public class AiCore(Kernel kernel, CoreInfo info, ILoggerFactory loggerFactory)
{
    private readonly ILogger<AiCore> _log = loggerFactory.CreateLogger<AiCore>();
    public string Name => info.Name;
    
    public override string ToString() => $"AI Core {Name}";
    
    public async Task<string> ChatAsync(string message, ChatHistory transcript, IDictionary<string, object> data)
    {
        _log.LogDebug("{CoreName}: Starting chat with message: {Message}", Name, message);
        
        IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        ChatHistory history = BuildChatHistory(message, transcript, data);
        history.LogHistory(_log);

        OllamaPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = kernel.Plugins.Count > 0 
                ? FunctionChoiceBehavior.Auto() 
                : FunctionChoiceBehavior.None(),
            ExtensionData = data
        };
        
        _log.LogDebug("{CoreName}: Sending message to chat service", Name);
        StringBuilder sb = new();
        foreach (var result in await chatService.GetChatMessageContentsAsync(history, settings, kernel))
        {
            string? content = result.Content;
            _log.LogDebug("{CoreName}: {Content}", Name, content);
            if (string.IsNullOrWhiteSpace(content)) continue;

            sb.Append(content);
        }

        return sb.ToString();
    }

    private ChatHistory BuildChatHistory(string message, ChatHistory transcript, IDictionary<string, object> data)
    {
        ChatHistory history = new();
        foreach (var instruction in info.Instructions)
        {
            history.AddSystemMessage(instruction.ResolveVariables(data));
        }

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