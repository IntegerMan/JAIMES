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
    
    public async IAsyncEnumerable<string> ChatAsync(string message)
    {
        _log.LogDebug("{CoreName}: Starting chat with message: {Message}", Name, message);
        
        IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        ChatHistory history = [];
        history.AddSystemMessage(info.Instructions);
        history.AddUserMessage(message);
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
}