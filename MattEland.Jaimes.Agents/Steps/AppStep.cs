using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Core.Services;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Steps;

[Experimental("SKEXP0080")]
public abstract class AppStep : KernelProcessStep
{
    protected async Task<T> EmitAsync<T>(string eventName, T data, KernelProcessStepContext steps, IConversationContextService conversationService) where T : class
    {
        conversationService.SetContext(data);
        await steps.EmitEventAsync(eventName, data);
        conversationService.Events.SendMessage(data);
        
        return data;
    }
}