using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Processes;

public class PlannerProcess
{
    [Experimental("SKEXP0080")]
    public static ProcessBuilder Create(bool includeEvaluation = true)
    {
        ProcessBuilder process = new("Planner");

        ProcessStepBuilder plannerStep = process.AddStepFromType<PlannerStep>();

        process.OnInputEvent(ProcessEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, parameterName: "conversation"));

        if (includeEvaluation)
        {
            ProcessStepBuilder planEvalStep = process.AddStepFromType<EvaluatePlanStep>();
            plannerStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(planEvalStep, parameterName: "plan"));
        }

        return process;
    }
}