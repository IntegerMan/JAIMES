using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;
#pragma warning disable SKEXP0080

namespace MattEland.Jaimes.Agents.Processes;

public class StandardAdventureProcess
{
    public static ProcessBuilder Create()
    {
        ProcessBuilder process = new("Standard-Adventure");

        ProcessStepBuilder plannerStep = process.AddStepFromType<PlannerStep>();
        ProcessStepBuilder composerStep = process.AddStepFromType<ComposerStep>();
        ProcessStepBuilder editorStep = process.AddStepFromType<EditorStep>();
        ProcessStepBuilder outputStep = process.AddStepFromType<DisplayMessageOutputStep>();
        ProcessStepBuilder inputStep = process.AddStepFromType<GetPlayerInputStep>();

        process.OnInputEvent(ProcessEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, parameterName: "conversation"))
            .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "conversation"))
            .SendEventTo(new ProcessFunctionTargetBuilder(editorStep, parameterName: "conversation"));
        plannerStep.OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "plan"));
        composerStep.OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(editorStep, parameterName: "draft"));
        editorStep.OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(outputStep, parameterName: "message"));
        outputStep.OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(inputStep, parameterName: "message"));
        inputStep.OnEvent(GetPlayerInputStep.InputReceivedEvent)
            .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep, parameterName: "conversation"))
            .SendEventTo(new ProcessFunctionTargetBuilder(composerStep, parameterName: "conversation"))
            .SendEventTo(new ProcessFunctionTargetBuilder(editorStep, parameterName: "conversation"));
        inputStep.OnEvent(GetPlayerInputStep.ExitRequestedEvent)
            .StopProcess();
        
        return process;
    }
}