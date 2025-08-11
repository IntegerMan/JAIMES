using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Steps;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Processes;

public class PlannerProcess
{
    [Experimental("SKEXP0080")]
    public ProcessBuilder Create()
    {
        ProcessBuilder process = new("Planner");

        ProcessStepBuilder plannerStep = process.AddStepFromType<PlannerStep>();
        process.OnInputEvent(ProcessEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(plannerStep));

        return process;
    }
}