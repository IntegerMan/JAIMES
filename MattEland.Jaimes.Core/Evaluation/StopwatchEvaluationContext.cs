using Microsoft.Extensions.AI.Evaluation;

namespace MattEland.Jaimes.Core.Evaluation;

public class StopwatchEvaluationContext(long elapsedMilliseconds) : EvaluationContext("StopwatchEvaluationContext")
{
    public long ElapsedMilliseconds => elapsedMilliseconds;
}