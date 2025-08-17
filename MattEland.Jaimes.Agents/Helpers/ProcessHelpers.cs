using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Serilog;

namespace MattEland.Jaimes.Agents.Helpers;


[Experimental("SKEXP0080")]
public static class ProcessHelpers
{
    public static async Task<T> EmitAsync<T>(this KernelProcessStepContext steps, string eventName, T data, Kernel kernel) where T : class
    {
        await steps.EmitEventAsync(eventName, data);
        
        IEventsService events = kernel.Services.GetRequiredService<IEventsService>();
        events.SendMessage(data);
        
        return data;
    }    
    
    public static void EmitError(this KernelProcessStepContext steps, string stepName, Exception ex, Kernel kernel)
    {
        Log.Error(ex, "An error occurred while executing the {StepName} step.", stepName);

        IEventsService events = kernel.Services.GetRequiredService<IEventsService>();
        events.SendMessage(new StepErrorMessage
        {
            StepName = stepName,
            Error = ex,
            Configuration = kernel.GetRequiredService<OrchestrationConfiguration>()
        });
    }
}