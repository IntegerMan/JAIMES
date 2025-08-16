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
public sealed class EditorStep : KernelProcessStep
{
    public const string RenderedHistoryKey = "Editor__RenderedHistory";
    public static string ReplyGeneratedEvent => "Editor__ReplyGenerated";

    [KernelFunction("Execute")]
    public async Task<ResponseFinalizedMessage> ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, ConversationMessage conversation, ResponseComposedMessage draft)
    {
        try
        {
            EditorAgent editor = new(kernel);
            ResponseFinalizedMessage result = await editor.GenerateAsync(conversation.History, draft.Response);
            
            IConversationContextService conversationContext = kernel.Services.GetRequiredService<IConversationContextService>();
            conversationContext.SetContext(RenderedHistoryKey, result.History);
            
            return await steps.EmitAsync(ReplyGeneratedEvent, result, kernel);
        }
        catch (Exception ex)
        {
            steps.EmitError(GetType().Name, ex, kernel);
            throw;
        }
    }
}