using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Domain;
using MattEland.Jaimes.Core.Evaluation;
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
    
    [KernelFunction]
    public async Task ExecuteAsync(KernelProcessStepContext steps, PlannerStepResult planResult, ConversationContext convContext)
    {
        try
        {
            IServiceProvider sp = convContext.ServiceProvider;
            EvaluationManager evaluationManager = sp.GetRequiredService<EvaluationManager>();
            ReportingConfiguration config = evaluationManager.BuildReportingConfig(); // TODO: This would be good to get from an active eval context
            EvaluationResult result = await EvaluationManager.EvaluateInteractionAsync(config, planResult.History, planResult.Response, "Planner Evaluation", iteration: "N/A");

            await steps.EmitEventAsync(EvaluatedEvent, result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the planner step.");
            throw;
        }
    }
}