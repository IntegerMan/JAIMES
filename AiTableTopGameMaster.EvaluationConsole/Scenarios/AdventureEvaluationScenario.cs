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

public abstract class AdventureEvaluationScenario : EvaluationScenario
{
    private readonly ServiceProvider _services;
    private readonly string _coreName;
    private readonly Adventure _adventure;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IAnsiConsole _console;
    private readonly CoreFactory _factory;
    private AiCore? _currentCore;

    protected AdventureEvaluationScenario(ServiceProvider services, string coreName)
    {
        _services = services;
        _coreName = coreName;
        Name = $"GameStart_{coreName}";
        
        IPromptsService promptsService = services.GetRequiredService<IPromptsService>();
        _adventure = services.GetRequiredService<Adventure>();
        Character character = services.GetRequiredService<Character>();
        _adventure.PlayerCharacter = character;
        
        IDictionary<string, object> data = _adventure.CreateChatData();
        Message = promptsService.GetInitialGreetingMessage(data);
        _loggerFactory = _services.GetRequiredService<ILoggerFactory>();
        _console = _services.GetRequiredService<IAnsiConsole>();
        _factory = _services.GetRequiredService<CoreFactory>();
    }

    public override string Name { get; }
    public override string Message { get; }

    public override async Task<ChatResult> GetResponseAsync(string message, string modelId)
    {
        ChatHistory history = new();

        IDictionary<string, object> data = _adventure.CreateChatData();

        IEnumerable<CoreInfo> coreInfo = _services.GetServices<CoreInfo>();
        CoreInfo info = coreInfo.First(c => c.Name == _coreName);

        AiCore core;
        if (string.IsNullOrWhiteSpace(modelId) || modelId == info.ModelId)
        {
            core = _factory.CreateCore(info);
        }
        else
        {
            core = _factory.CreateCore(info with { ModelId = modelId });
        }
        _currentCore = core;

        ConsoleChatClient client = new(_console, [core], _loggerFactory);
        
        return await client.ChatAsync(message, history, data);
    }

    public override IEnumerable<string> AdditionalTags
    {
        get
        {
            yield return _adventure.Name;
            yield return _adventure.PlayerCharacter!.Name;
            if (_currentCore != null)
            {
                yield return _currentCore.ModelId;
                yield return _currentCore.Name;
            }
        }
    }
}