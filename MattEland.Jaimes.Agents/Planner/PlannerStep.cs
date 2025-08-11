using System.Diagnostics.CodeAnalysis;
using MattEland.Jaimes.Core.Models;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using Serilog;
using ConversationContext = MattEland.Jaimes.Core.Domain.ConversationContext;

namespace MattEland.Jaimes.Agents.Planner;

[Experimental("SKEXP0080")]
public sealed class PlannerStep : KernelProcessStep
{
    public static string PlanGeneratedEvent => "PlanGenerated";
    public static string RenderedHistoryKey => "PlannerHistory";

    [KernelFunction]
    public async Task ExecuteAsync(KernelProcessStepContext steps, ConversationContext convContext)
    {
        try
        {
            IServiceProvider sp = convContext.ServiceProvider;
            IConversationContextService conversationService = sp.GetRequiredService<IConversationContextService>();
            IModelFactory modelFactory = sp.GetRequiredService<IModelFactory>();
            IKernelBuilder kernelBuilder = sp.GetRequiredService<IKernelBuilder>();
            ModelInfo model = modelFactory.FindModel("qwen3:4b"); // TODO: This should come from convContext somewhere
            PlannerAgent planner = new(modelFactory, kernelBuilder, model);
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