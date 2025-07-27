using AiTableTopGameMaster.ConsoleShared.Clients;
using AiTableTopGameMaster.Core.Cores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.EvaluationConsole.Scenarios;

public class TestCoreEvaluationScenario : EvaluationScenario
{
    private readonly ConsoleChatClient _client;
    private readonly CoreInfo _coreInfo;

    public TestCoreEvaluationScenario(ServiceProvider services)
    {
        IAnsiConsole console = services.GetRequiredService<IAnsiConsole>();
        IKernelBuilder builder = services.GetRequiredService<IKernelBuilder>();
        ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();

        _coreInfo = new()
        {
            Name = "Test Core",
            Instructions = ["Answer the user's question in one or two sentences."],
            IncludeHistory = true,
            IncludePlayerInput = true,
            Plugins = []
        };
        List<CoreInfo> info =
        [
            _coreInfo
        ];
    
        Kernel kernel = builder.Build();
        List<AiCore> cores = info.Select(i => new AiCore(kernel, i, loggerFactory)).ToList();
    
        _client = new ConsoleChatClient(console, cores, loggerFactory);
    }

    public override string Name => "Test Core Evaluation Scenario";
    public override string Message => "What is the meaning of life?";
    
    public override async Task<string> GetResponseAsync(string message)
    {
        ChatHistory history = new();
        foreach (var instruction in _coreInfo.Instructions)
        {
            history.AddSystemMessage(instruction);
        }
        
        IDictionary<string, object> data = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["UserMessage"] = message,
            ["InputMessage"] = message
        };
        
        return await _client.ChatAsync(message, history, data);
    }
}