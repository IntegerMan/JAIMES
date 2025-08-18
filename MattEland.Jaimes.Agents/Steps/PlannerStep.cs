using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Functions;
using MattEland.Jaimes.Agents.Helpers;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Serilog;

namespace MattEland.Jaimes.Agents.Steps;

[Experimental("SKEXP0080")]
public sealed class PlannerStep : KernelProcessStep
{
    public const string RenderedHistoryKey = "Planner__RenderedHistory";
    public static string PlanGeneratedEvent => "Planner__PlanGenerated";

    [KernelFunction("Execute")]
    public async Task<PlanCompleteMessage> ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, IHasHistory conversation)
    {
        try
        {
            PlannerAgent planner = new(kernel);
            OrchestrationConfiguration configuration = kernel.GetRequiredService<OrchestrationConfiguration>();
            PlanCompleteMessage result = await planner.GenerateAsync(configuration, conversation.History);
            
            IConversationContextService conversationContext = kernel.Services.GetRequiredService<IConversationContextService>();
            conversationContext.SetContext(RenderedHistoryKey, result.History);
            
            return await steps.EmitAsync(PlanGeneratedEvent, result, kernel);
        }
        catch (Exception ex)
        {
            steps.EmitError(GetType().Name, ex, kernel);
            throw;
        }
    }
}