using AiTableTopGameMaster.ConsoleShared.Clients;
using Microsoft.Extensions.AI.Evaluation;

namespace AiTableTopGameMaster.EvaluationConsole.Scenarios;

public abstract class EvaluationScenario
{
    public abstract string Name { get; }
    public abstract string Message { get; }

    public IEnumerable<EvaluationContext> BuildContext(ChatResult result)
    {
        return [new StopwatchEvaluationContext(result.EllapsedMilliseconds)];
    }

    public abstract Task<ChatResult> GetResponseAsync(string message);

    public abstract IEnumerable<string> AdditionalTags { get; }
}