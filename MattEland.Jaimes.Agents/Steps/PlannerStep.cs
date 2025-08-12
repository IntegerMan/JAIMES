using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Functions;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Serilog;

namespace MattEland.Jaimes.Agents.Steps;

[Experimental("SKEXP0080")]
public sealed class PlannerStep : AppStep
{
    public const string RenderedHistoryKey = "Planner__RenderedHistory";
    public static string PlanGeneratedEvent => "Planner__PlanGenerated";

    [KernelFunction("Execute")]
    public async Task<PlanCompleteMessage> ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, ConversationMessage conversation)
    {
        try
        {
            IServiceProvider sp = conversation.ServiceProvider;
            IConversationContextService conversationService = sp.GetRequiredService<IConversationContextService>();
            PlannerAgent planner = new(kernel);
            PlanCompleteMessage result = await planner.GenerateAsync(conversation);
            
            conversationService.SetContext(RenderedHistoryKey, result.History);
            return await EmitAsync(PlanGeneratedEvent, result, steps, conversationService);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the planner step.");
            throw;
        }
    }
}