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
        
        if (includeEvaluation)
        {
            ProcessStepBuilder planEvalStep = process.AddStepFromType<EvaluatePlanStep>();
            plannerStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(planEvalStep, parameterName: "plan"));
            
            ProcessStepBuilder composeEvalStep = process.AddStepFromType<EvaluateMessageStep>(id: "ComposeEval");
            composerStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(composeEvalStep, parameterName: "message"));
            
            ProcessStepBuilder editorEvalStep = process.AddStepFromType<EvaluateMessageStep>(id: "EditorEval");
            editorStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(editorEvalStep, parameterName: "message"));
        }
        
        return process;
    }
}