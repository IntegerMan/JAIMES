using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Processes;

public class PlannerProcess
{
    [Experimental("SKEXP0080")]
    public static ProcessBuilder Create()
    {
        ProcessBuilder process = new("Planner");

        ProcessStepBuilder plannerStep = process.AddStepFromType<PlannerStep>();
        ProcessStepBuilder planEvalStep = process.AddStepFromType<EvaluatePlanStep>();
        
        process.OnInputEvent(ProcessEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep));
        process.OnEvent(PlannerStep.PlanGeneratedEvent)
            .SendEventTo(new ProcessFunctionTargetBuilder(planEvalStep));

        return process;
    }
}