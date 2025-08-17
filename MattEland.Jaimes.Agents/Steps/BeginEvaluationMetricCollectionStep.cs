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
public class BeginEvaluationMetricCollectionStep : KernelProcessStep
{
    [KernelFunction("Execute")]
    public void Execute(Kernel kernel, KernelProcessStepContext context)
    {
        try
        {
            EvaluationManager evaluationManager = kernel.Services.GetRequiredService<EvaluationManager>();
            evaluationManager.BuildReportingConfig(kernel);
        }
        catch (Exception ex)
        {
            context.EmitError(GetType().Name, ex, kernel);
            throw;
        }
    }
}