using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace AiTableTopGameMaster.EvaluationConsole;

public class StopwatchEvaluator : IEvaluator
{
    public ValueTask<EvaluationResult> EvaluateAsync(
        IEnumerable<ChatMessage> messages, 
        ChatResponse modelResponse, 
        ChatConfiguration? chatConfiguration = null, 
        IEnumerable<EvaluationContext>? additionalContext = null,
        CancellationToken cancellationToken = new())
    {
        StopwatchEvaluationContext context = additionalContext?.OfType<StopwatchEvaluationContext>().FirstOrDefault() 
            ?? throw new ArgumentException("StopwatchEvaluationContext is required for StopwatchEvaluator");

        NumericMetric msMetric = new("ElapsedMilliseconds", context.ElapsedMilliseconds)
        {
            Interpretation = new EvaluationMetricInterpretation(EvaluationRating.Inconclusive, failed: false, reason: "Time taken for evaluation")
        };
        
        return ValueTask.FromResult(new EvaluationResult(msMetric));
    }

    public IReadOnlyCollection<string> EvaluationMetricNames => ["ElapsedMilliseconds"];
}