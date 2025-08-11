using System.Text.Json;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0001

namespace MattEland.Jaimes.Agents.Functions;

public class PlannerAgent(Kernel kernel)
{
    public string Name => "Planner";
    public string[] Plugins => [];
    
    public async Task<PlannerStepResult> GenerateAsync(ChatHistory history)
    {
        PlannerResponse sampleResponse = new()
        {
            Checks = "None, or actions to request from the player (e.g. roll a skill check, clarify something, etc.)",
            KeyPoints = ["A list of key points that must be included in the storyteller's response."],
            Cautions = "Any cautions or warnings to convey to the storyteller for use when generating a response"
        };
        
        ChatHistory messages = [];
        messages.AddSystemMessage("""
                                  You are a planning agent that creates a structured plan for a storyteller AI based on the conversation history provided.
                                  Your task is to analyze the conversation history and create a detailed plan that outlines the steps needed to achieve the user's objectives.
                                  The plan should be in JSON format with the following structure:
                                  """);
        messages.AddSystemMessage(JsonSerializer.Serialize(sampleResponse));
        messages.AddSystemMessage("""
                                  Ensure that the plan is clear, actionable, and takes into account any constraints or preferences mentioned by the user.
                                  """);
        history.CopyMessagesTo(messages, AuthorRole.Assistant, AuthorRole.User);
        
        IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
        
        // NOTE: Ollama doesn't support structured output via the API in Semantic Kernel, but this way seems to work
        // We're not using tools or Semantic Kernel in this, but it's viable for strongly-typed responses
        IChatClient chatClient = chatService.AsChatClient();
        ChatResponse<PlannerResponse> response = 
            await chatClient.GetResponseAsync<PlannerResponse>(messages.ToChatMessages());

        return new PlannerStepResult
        {
            History = messages,
            Plan = response.Result,
            Response = response
        };
    }
}