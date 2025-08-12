using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Helpers;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Domain;
using MattEland.Jaimes.Core.Evaluation;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Serilog;

namespace MattEland.Jaimes.Agents.Steps;

[Experimental("SKEXP0080")]
public class EvaluatePlanStep : KernelProcessStep
{
    public static string EvaluatedEvent => "PlanEvaluated";
    
    [KernelFunction("Execute")]
    public async Task<PlanEvaluatedMessage> ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, PlanCompleteMessage plan)
    {
        try
        {
            EvaluationManager evaluationManager = kernel.Services.GetRequiredService<EvaluationManager>();
            ReportingConfiguration config = evaluationManager.BuildReportingConfig(); // TODO: This would be good to get from an active eval context
            EvaluationResult result = await EvaluationManager.EvaluateInteractionAsync(config, plan.History, plan.Response, "PlannerEval", iteration: "NA");
            PlanEvaluatedMessage evaluatedMessage = new(plan.Plan, result);

            return await steps.EmitAsync(EvaluatedEvent, evaluatedMessage, kernel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the planner step.");
            throw;
        }
    }
}