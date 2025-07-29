using Microsoft.Extensions.AI.Evaluation;

namespace AiTableTopGameMaster.EvaluationConsole.Scenarios;

public abstract class EvaluationScenario
{
    public abstract string Name { get; }
    public abstract string Message { get; }

    public virtual EvaluationContext? BuildContext()
    {
        return null;
    }

    public abstract Task<string> GetResponseAsync(string message);

    public abstract IEnumerable<string> AdditionalTags { get; }
}