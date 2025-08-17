using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;
#pragma warning disable SKEXP0080

namespace MattEland.Jaimes.Agents.Processes;

public class PlanComposeEditProcess
{
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
            ProcessStepBuilder beginEvalStep = process.AddStepFromType<BeginEvaluationMetricCollectionStep>();
            ProcessStepBuilder planEvalStep = process.AddStepFromType<EvaluatePlanStep>();
            ProcessStepBuilder composeEvalStep = process.AddStepFromType<EvaluateMessageStep>(id: "ComposeEval");
            ProcessStepBuilder editorEvalStep = process.AddStepFromType<EvaluateMessageStep>(id: "EditorEval");
            ProcessStepBuilder buildEvalReportStep = process.AddStepFromType<BuildEvaluationReportStep>();
            
            process.OnInputEvent(ProcessEvents.StartProcess)
                .SendEventTo(new ProcessFunctionTargetBuilder(beginEvalStep));

            beginEvalStep.OnFunctionResult() 
                .SendEventTo(new ProcessFunctionTargetBuilder(planEvalStep));

            plannerStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(planEvalStep, parameterName: "plan"));
            
            planEvalStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(composeEvalStep));
            
            composerStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(composeEvalStep, parameterName: "message"));
                
            composeEvalStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(editorEvalStep));
            
            editorStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(editorEvalStep, parameterName: "message"));

            editorEvalStep.OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(buildEvalReportStep));
        }
        
        return process;
    }
}