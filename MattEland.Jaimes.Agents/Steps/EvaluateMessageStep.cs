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
public class EvaluateMessageStep : KernelProcessStep
{
    public static string EvaluatedEvent => "MessageEvaluated";
    
    [KernelFunction("Execute")]
    public async Task<ResponseEvaluatedMessage> ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, IResponseMessage message)
    {
        string stepName = $"{message.StepName} Evaluation";

        try
        {
            OrchestrationConfiguration configuration = kernel.GetRequiredService<OrchestrationConfiguration>();
            EvaluationManager eval = kernel.Services.GetRequiredService<EvaluationManager>();
            EvaluationResult result = await eval.EvaluateInteractionAsync(message.History, message.Response, stepName, iteration: "NA");
            ResponseEvaluatedMessage evaluatedMessage = new()
            {
                Configuration = configuration,
                History = message.History,
                Response = message.Response,
                Evaluation = result,
                StepName = stepName,
                ServiceId = configuration.ModelServiceAssignments["Evaluator"]
            };

            return await steps.EmitAsync(EvaluatedEvent, evaluatedMessage, kernel);
        }
        catch (Exception ex)
        {
            steps.EmitError(stepName, ex, kernel);
            throw;
        }
    }
}