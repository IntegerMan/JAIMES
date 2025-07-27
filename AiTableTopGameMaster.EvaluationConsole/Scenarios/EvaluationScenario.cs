using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
#pragma warning disable AIEVAL001

namespace AiTableTopGameMaster.EvaluationConsole.Scenarios;

public abstract class EvaluationScenario
{
    public abstract string Name { get; }
    public abstract string Message { get; }

    public virtual IEnumerable<IEvaluator> BuildEvaluators()
    {
        yield return new CoherenceEvaluator();
        //new CompletenessEvaluator(),
        yield return new FluencyEvaluator();
        //new GroundednessEvaluator(),
        yield return new RelevanceEvaluator();
        yield return new RelevanceTruthAndCompletenessEvaluator();
        //new EquivalenceEvaluator(),
        //new RetrievalEvaluator()
    }

    public virtual EvaluationContext? BuildContext()
    {
        return null;
    }

    public abstract Task<string> GetResponseAsync(string message);
}