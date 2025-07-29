using Microsoft.Extensions.DependencyInjection;

namespace AiTableTopGameMaster.EvaluationConsole.Scenarios;

public class PlannerEvaluationScenario(ServiceProvider services) 
    : AdventureEvaluationScenario(services, "Planner")
{
    public override string Name => "GameStart_Planner";
}