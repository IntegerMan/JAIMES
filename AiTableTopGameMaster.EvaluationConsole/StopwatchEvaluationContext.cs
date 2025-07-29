using Microsoft.Extensions.AI.Evaluation;

namespace AiTableTopGameMaster.EvaluationConsole;

public class StopwatchEvaluationContext(long elapsedMilliseconds) : EvaluationContext("StopwatchEvaluationContext")
{
    public long ElapsedMilliseconds => elapsedMilliseconds;
}