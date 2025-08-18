using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Helpers;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
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
    
    [KernelFunction("Execute")]
    public async Task<PlanEvaluatedMessage> ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, PlanCompleteMessage plan)
    {
        try
        {
            EvaluationManager eval = kernel.Services.GetRequiredService<EvaluationManager>();
            EvaluationResult result = await eval.EvaluateInteractionAsync(plan.History, plan.Response, "PlannerEval", iteration: "NA");
            OrchestrationConfiguration configuration = kernel.GetRequiredService<OrchestrationConfiguration>();
            string serviceId = configuration.ModelServiceAssignments["Evaluator"];
            PlanEvaluatedMessage evaluatedMessage = new(plan.Plan, result, serviceId)
            {
                Configuration = configuration
            };

            return await steps.EmitAsync(EvaluatedEvent, evaluatedMessage, kernel);
        }
        catch (Exception ex)
        {
            steps.EmitError(GetType().Name, ex, kernel);
            throw;
        }
    }
}