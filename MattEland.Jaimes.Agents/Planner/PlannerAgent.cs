using AiTableTopGameMaster.Core.Models;
using MattEland.Jaimes.RAG;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Planner;


public class PlannerAgent(ITranscriptService transcriptService, IModelFactory factory, IKernelBuilder builder, ModelInfo model) : IPlannerAgent
{
    public string Name => "Planner";
    public string[] Plugins => [];
    
    public async Task<PlannerResponse> GenerateAsync()
    {
        ChatHistory history = transcriptService.GetChatHistory();
        factory.ConfigureKernel(builder, Name, model, Plugins);
        Kernel kernel = builder.Build();
        
        string promptTemplate = """
            You are a planning agent that creates a structured plan for a storyteller AI based on the conversation history provided.
            Given the following conversation history, create a detailed plan that outlines the steps needed to achieve the user's objectives.
            The plan should be in JSON format with the following structure:
            {
                "checks": "None, or actions to request from the player (e.g. roll a skill check, clarify something, etc.)",
                "keyPoints": "A list of key points that must be included in the storyteller's response.",
                "cautions": "Any cautions or warnings to convey to the storyteller for use when generating a response",
            }
            Ensure that the plan is clear, actionable, and takes into account any constraints or preferences mentioned by the user.
            """;

        // TODO: Include relevant context from history in the prompt
        
        PlannerResponse? result = await kernel.InvokePromptAsync<PlannerResponse>(promptTemplate);
        
        return result ?? throw new InvalidOperationException("The planner did not return a response.");
    }
}