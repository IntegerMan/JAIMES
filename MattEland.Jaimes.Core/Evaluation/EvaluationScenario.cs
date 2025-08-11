using MattEland.Jaimes.Core.Cores;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace MattEland.Jaimes.Core.Evaluation;

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

    public virtual IEnumerable<string> AdditionalTags { get; } = [];
}