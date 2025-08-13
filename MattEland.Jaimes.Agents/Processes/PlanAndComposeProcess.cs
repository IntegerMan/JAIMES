using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Processes;

public class PlanAndComposeProcess
{
    [Experimental("SKEXP0080")]
    public static ProcessBuilder Create()
    {
        ProcessBuilder process = new("Planner-Composer");

        ProcessStepBuilder plannerStep = process.AddStepFromType<PlannerStep>();
        ProcessStepBuilder composerStep = process.AddStepFromType<ComposerStep>();

        process.OnInputEvent(ProcessEvents.StartProcess)
               .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, parameterName: "conversation"))
               .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "conversation"));
        plannerStep.OnFunctionResult()
                   .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "plan"));
        
        return process;
    }
}