using AiTableTopGameMaster.Core.Models;
using MattEland.Jaimes.RAG;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents;


public class PlannerAgent(ITranscriptService transcriptService, IModelFactory factory, IKernelBuilder builder, ModelInfo model) : IPlannerAgent
{
    public string Name => "Planner";
    public string[] Plugins => [];
    
    public Task<PlannerResponse> GenerateAsync()
    {
        ChatHistory history = transcriptService.GetChatHistory();
        factory.ConfigureKernel(builder, Name, model, Plugins);
        
        return Task.FromResult(new PlannerResponse
        {
            Checks = "NOT IMPLEMENTED",
            KeyPoints = "NOT IMPLEMENTED",
            Cautions = "NOT IMPLEMENTED"
        });
    }
}