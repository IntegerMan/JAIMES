using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Functions;
using MattEland.Jaimes.Agents.Helpers;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Serilog;

namespace MattEland.Jaimes.Agents.Steps;

[Experimental("SKEXP0080")]
public sealed class ComposerStep : KernelProcessStep
{
    public const string RenderedHistoryKey = "Composer__RenderedHistory";
    public static string ReplyGeneratedEvent => "Composer__ReplyGenerated";

    [KernelFunction("Execute")]
    public async Task<ResponseComposedMessage> ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, ConversationMessage conversation, PlanCompleteMessage plan)
    {
        try
        {
            ComposerAgent composer = new(kernel);
            ResponseComposedMessage result = await composer.GenerateAsync(conversation.History, plan.Plan);
            
            IConversationContextService conversationContext = kernel.Services.GetRequiredService<IConversationContextService>();
            conversationContext.SetContext(RenderedHistoryKey, result.History);
            
            return await steps.EmitAsync(ReplyGeneratedEvent, result, kernel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the composer step.");
            throw;
        }
    }
}