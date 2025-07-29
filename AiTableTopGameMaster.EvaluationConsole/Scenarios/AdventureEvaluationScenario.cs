using AiTableTopGameMaster.ConsoleShared.Clients;
using AiTableTopGameMaster.Core.Cores;
using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Helpers;
using AiTableTopGameMaster.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.EvaluationConsole.Scenarios;

public class AdventureEvaluationScenario : EvaluationScenario
{
    private readonly ConsoleChatClient _client;
    private readonly AiCore _core;
    private readonly Adventure _adventure;

    public AdventureEvaluationScenario(ServiceProvider services, string coreName)
    {
        Name = $"GameStart_{coreName}";
        
        IPromptsService promptsService = services.GetRequiredService<IPromptsService>();
        _adventure = services.GetRequiredService<Adventure>();
        Character character = services.GetRequiredService<Character>();
        _adventure.PlayerCharacter = character;
        
        IDictionary<string, object> data = _adventure.CreateChatData();
        Message = promptsService.GetInitialGreetingMessage(data);

        IAnsiConsole console = services.GetRequiredService<IAnsiConsole>();
        ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();

        IEnumerable<AiCore> cores = services.GetServices<AiCore>();
        
        _core = cores.First(c => c.Name == coreName);
        _client = new ConsoleChatClient(console, [_core], loggerFactory);
    }

    public override string Name { get; }
    public override string Message { get; }

    public override async Task<ChatResult> GetResponseAsync(string message)
    {
        ChatHistory history = new();

        IDictionary<string, object> data = _adventure.CreateChatData();
        return await _client.ChatAsync(message, history, data);
    }

    public override IEnumerable<string> AdditionalTags
    {
        get
        {
            yield return _adventure.Name;
            yield return _adventure.PlayerCharacter!.Name;
            yield return _core.ModelId;
            yield return _core.Name;
        }
    }
}