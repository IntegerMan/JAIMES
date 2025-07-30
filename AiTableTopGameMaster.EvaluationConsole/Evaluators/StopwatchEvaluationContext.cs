using Microsoft.Extensions.AI.Evaluation;

namespace AiTableTopGameMaster.EvaluationConsole.Evaluators;

public class StopwatchEvaluationContext(long elapsedMilliseconds) : EvaluationContext("StopwatchEvaluationContext")
{
    public long ElapsedMilliseconds => elapsedMilliseconds;
}