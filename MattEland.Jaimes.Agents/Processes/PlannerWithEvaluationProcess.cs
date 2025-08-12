using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Processes;

public class PlannerWithEvaluationProcess
{
    [Experimental("SKEXP0080")]
    public static ProcessBuilder Create()
    {
        ProcessBuilder process = new("Planner");

        ProcessStepBuilder plannerStep = process.AddStepFromType<PlannerStep>();
        ProcessStepBuilder planEvalStep = process.AddStepFromType<EvaluatePlanStep>();

        process.OnInputEvent(ProcessEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, parameterName: "conversation"))
            .SendEventTo(new ProcessFunctionTargetBuilder(planEvalStep, parameterName: "conversation"));
        plannerStep.OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(planEvalStep, parameterName: "plan"));
        
        return process;
    }
}