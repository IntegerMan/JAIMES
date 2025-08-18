using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Helpers;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Core.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Steps;

[Experimental("SKEXP0080")]
public class DisplayMessageOutputStep : KernelProcessStep
{
    [KernelFunction("Execute")]
    public IResponseMessage Execute(Kernel kernel, KernelProcessStepContext steps, IResponseMessage message)
    {
        try
        {
            IChatInterface ui = kernel.Services.GetRequiredService<IChatInterface>();
            ui.DisplayAgentMessage(message.Response);

            return message;
        }
        catch (Exception ex)
        {
            steps.EmitError(GetType().Name, ex, kernel);
            throw;
        }
    }
}