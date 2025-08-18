using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Processes;

public class PlanAndComposeProcess
{
    [Experimental("SKEXP0080")]
    public static ProcessBuilder Create(bool includeEvaluation)
    {
        ProcessBuilder process = new("Planner-Composer");

        ProcessStepBuilder plannerStep = process.AddStepFromType<PlannerStep>();
        ProcessStepBuilder composerStep = process.AddStepFromType<ComposerStep>();

        process.OnInputEvent(ProcessEvents.StartProcess)
               .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, parameterName: "conversation"))
               .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "conversation"));
        plannerStep.OnFunctionResult()
                   .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "plan"));
        
        if (includeEvaluation)
        {
            ProcessStepBuilder beginEvalStep = process.AddStepFromType<BeginEvaluationMetricCollectionStep>();
            ProcessStepBuilder planEvalStep = process.AddStepFromType<EvaluatePlanStep>();
            ProcessStepBuilder composeEvalStep = process.AddStepFromType<EvaluateMessageStep>(id: "ComposeEval");
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
                .SendEventTo(new ProcessFunctionTargetBuilder(buildEvalReportStep));
        }
        
        return process;
    }
}