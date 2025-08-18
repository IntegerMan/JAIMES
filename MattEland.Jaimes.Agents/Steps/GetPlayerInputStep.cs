using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Agents.Helpers;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Core.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Steps;

[Experimental("SKEXP0080")]
public class GetPlayerInputStep : KernelProcessStep
{
    public const string InputReceivedEvent = "PlayerInput__InputReceived";
    public const string ExitRequestedEvent = "PlayerInput__ExitRequested";
    
    [KernelFunction("Execute")]
    public async Task<PlayerInputMessage> ExecuteAsync(Kernel kernel, KernelProcessStepContext steps, IResponseMessage message)
    {
        try
        {
            IChatInterface ui = kernel.Services.GetRequiredService<IChatInterface>();
            string input = await ui.GetPlayerInputAsync();

            ChatHistory history = new(message.History);
            history.AddUserMessage(input);

            PlayerInputMessage inputMessage = new()
            {
                Configuration = message.Configuration,
                History = history,
                Response = input,
                StepName = nameof(GetPlayerInputStep)
            };
            
            // Firing different events based on different input classes allows processes to fork
            if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                await steps.EmitAsync(ExitRequestedEvent, inputMessage, kernel);
            }
            else
            {
                await steps.EmitAsync(InputReceivedEvent, inputMessage, kernel);
            }

            return inputMessage;
        }
        catch (Exception ex)
        {
            steps.EmitError(GetType().Name, ex, kernel);
            throw;
        }
    }
}