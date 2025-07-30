using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;

namespace AiTableTopGameMaster.EvaluationConsole.Evaluators;

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

        EvaluationRating rating = context.ElapsedMilliseconds switch
        {
            < 700 => EvaluationRating.Exceptional,
            < 1000 => EvaluationRating.Good,
            < 1500 => EvaluationRating.Average,
            < 2000 => EvaluationRating.Poor,
            _ => EvaluationRating.Unacceptable
        };
        NumericMetric msMetric = new("ElapsedMilliseconds", context.ElapsedMilliseconds)
        {
            Interpretation = new EvaluationMetricInterpretation(rating, 
                failed: rating is EvaluationRating.Poor or EvaluationRating.Unacceptable, 
                reason: "Time taken for evaluation")
        };
        
        return ValueTask.FromResult(new EvaluationResult(msMetric));
    }

    public IReadOnlyCollection<string> EvaluationMetricNames => ["ElapsedMilliseconds"];
}