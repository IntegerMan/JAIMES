using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Processes;

public class PlanComposeEditProcess
{
    [Experimental("SKEXP0080")]
    public static ProcessBuilder Create(bool includeEvaluation)
    {
        ProcessBuilder process = new("Plan-Compose-Edit");

        ProcessStepBuilder plannerStep = process.AddStepFromType<PlannerStep>();
        ProcessStepBuilder composerStep = process.AddStepFromType<ComposerStep>();
        ProcessStepBuilder editorStep = process.AddStepFromType<EditorStep>();

        process.OnInputEvent(ProcessEvents.StartProcess)
               .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, parameterName: "conversation"))
               .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "conversation"))
               .SendEventTo(new ProcessFunctionTargetBuilder(editorStep, parameterName: "conversation"));
        plannerStep.OnFunctionResult()
                   .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "plan"));
        composerStep.OnFunctionResult()
                    .SendEventTo(new ProcessFunctionTargetBuilder(editorStep, parameterName: "draft"));
        
        return process;
    }
}