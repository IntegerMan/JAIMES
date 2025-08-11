using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Core.Models;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Serilog;

namespace MattEland.Jaimes.Agents.Planner;

[Experimental("SKEXP0080")]
public sealed class PlannerStep(KernelContextService contextService) : KernelProcessStep
{
    public static string PlanGeneratedEvent { get; } = "PlanGenerated";
    
    [KernelFunction]
    public async Task ExecuteAsync(KernelProcessStepContext context, ChatHistory history)
    {
        try
        {
            IServiceProvider sp = contextService.ServiceProvider;
            IConversationContextService conversationService = sp.GetRequiredService<IConversationContextService>();
            IModelFactory modelFactory = sp.GetRequiredService<IModelFactory>();
            IKernelBuilder kernelBuilder = sp.GetRequiredService<IKernelBuilder>();
            ModelInfo model = modelFactory.FindModel("qwen3:4b"); // TODO: This should be an input parameter or configuration setting
            PlannerAgent planner = new PlannerAgent(modelFactory, kernelBuilder, model);
            PlannerResponse result = await planner.GenerateAsync(history);
            conversationService.SetContext(result);
            await context.EmitEventAsync(PlanGeneratedEvent, result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while executing the planner step.");
        }
    }
}