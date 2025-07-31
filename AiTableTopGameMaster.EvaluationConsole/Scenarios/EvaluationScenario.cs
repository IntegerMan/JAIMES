using AiTableTopGameMaster.Core.Cores;
using AiTableTopGameMaster.EvaluationConsole.Evaluators;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace AiTableTopGameMaster.EvaluationConsole.Scenarios;

public abstract class EvaluationScenario
{
    public abstract string Name { get; }
    public abstract string Message { get; }

    public IEnumerable<EvaluationContext> BuildContext(ChatResult result)
    {
        return [
            new StopwatchEvaluationContext(result.ElapsedMilliseconds),
            new CompletenessEvaluatorContext(CompletenessGroundTruth),
            new EquivalenceEvaluatorContext(EquivalenceGroundTruth),
        ];
    }

    protected abstract string CompletenessGroundTruth { get; }
    protected abstract string EquivalenceGroundTruth { get; }

    public abstract Task<ChatResult> GetResponseAsync(string message, string modelId);

    public abstract IEnumerable<string> AdditionalTags { get; }
}