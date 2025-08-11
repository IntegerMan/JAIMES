using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Definitions;
using MattEland.Jaimes.Agents.Functions;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Serilog;
using ConversationContext = MattEland.Jaimes.Core.Domain.ConversationContext;

namespace MattEland.Jaimes.Agents.Steps;

[Experimental("SKEXP0080")]
public sealed class PlannerStep : KernelProcessStep
{
    public static string PlanGeneratedEvent => "PlanGenerated";
    public static string RenderedHistoryKey => "PlannerHistory";

    [KernelFunction]
    public async Task ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, ConversationContext convContext)
    {
        try
        {
            IServiceProvider sp = convContext.ServiceProvider;
            IConversationContextService conversationService = sp.GetRequiredService<IConversationContextService>();
            PlannerAgent planner = new(kernel);
            (PlannerResponse result, ChatHistory renderedHistory) = await planner.GenerateAsync(convContext.History);
            conversationService.SetContext(result);
            conversationService.SetContext(RenderedHistoryKey, renderedHistory);
            await steps.EmitEventAsync(PlanGeneratedEvent, result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the planner step.");
        }
    }
}