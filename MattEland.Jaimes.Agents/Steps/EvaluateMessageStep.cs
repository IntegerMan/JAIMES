using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Helpers;
using MattEland.Jaimes.Agents.Messages;
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
            EvaluationManager eval = kernel.Services.GetRequiredService<EvaluationManager>();
            EvaluationResult result = await eval.EvaluateInteractionAsync(message.History, message.Response, stepName, iteration: "NA");
            ResponseEvaluatedMessage evaluatedMessage = new(message.History, message.Response, result, message.StepName);

            return await steps.EmitAsync(EvaluatedEvent, evaluatedMessage, kernel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the {StepName} step.", stepName);
            throw;
        }
    }
}