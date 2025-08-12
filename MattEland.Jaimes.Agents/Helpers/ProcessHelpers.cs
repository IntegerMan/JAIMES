using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Helpers;

public static class ProcessHelpers
{
    [Experimental("SKEXP0080")]
    public static async Task<T> EmitAsync<T>(this KernelProcessStepContext steps, string eventName, T data, Kernel kernel) where T : class
    {
        await steps.EmitEventAsync(eventName, data);
        
        IEventsService events = kernel.Services.GetRequiredService<IEventsService>();
        events.SendMessage(data);
        
        return data;
    }
}