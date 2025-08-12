using System.Diagnostics.CodeAnalysis;
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
public class EvaluatePlanStep : AppStep
{
    public static string EvaluatedEvent => "PlanEvaluated";
    
    [KernelFunction("Execute")]
    public async Task<PlanEvaluatedMessage> ExecuteAsync(KernelProcessStepContext steps, PlanCompleteMessage plan, ConversationMessage conversation)
    {
        try
        {
            IServiceProvider sp = conversation.ServiceProvider;
            EvaluationManager evaluationManager = sp.GetRequiredService<EvaluationManager>(); // Alternatively, we could get this from the kernel, but it's nice to use a separate one
            ReportingConfiguration config = evaluationManager.BuildReportingConfig(); // TODO: This would be good to get from an active eval context
            EvaluationResult result = await EvaluationManager.EvaluateInteractionAsync(config, plan.History, plan.Response, "PlannerEval", iteration: "NA");
            PlanEvaluatedMessage evaluatedMessage = new(plan.Plan, result);
            return await EmitAsync(EvaluatedEvent, evaluatedMessage, steps, sp.GetRequiredService<IConversationContextService>());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the planner step.");
            throw;
        }
    }
}