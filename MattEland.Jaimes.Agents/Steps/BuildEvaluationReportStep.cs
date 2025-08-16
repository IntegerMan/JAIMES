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
public class BuildEvaluationReportStep : KernelProcessStep
{
    [KernelFunction("Execute")]
    public async Task Execute(Kernel kernel, KernelProcessStepContext context)
    {
        try
        {
            EvaluationManager evaluationManager = kernel.Services.GetRequiredService<EvaluationManager>();
            await evaluationManager.ExportEvaluationReportAsync(Environment.CurrentDirectory, openInBrowser: true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the {StepName} step.", GetType().Name);
            throw;
        }
    }
}