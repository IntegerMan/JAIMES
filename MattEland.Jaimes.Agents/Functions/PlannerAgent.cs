using System.Text.Json;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

#pragma warning disable SKEXP0001

namespace MattEland.Jaimes.Agents.Functions;

public class PlannerAgent(Kernel kernel)
{
    public string Name => "Planner";
    public string[] Plugins => [];
    
    public async Task<PlanCompleteMessage> GenerateAsync(ConversationMessage conversation)
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
        conversation.History.CopyMessagesTo(messages, AuthorRole.Assistant, AuthorRole.User);


        // TODO: This should come from a generic factory so it's not tied to an implementation
        PromptExecutionSettings executionSettings = new OpenAIPromptExecutionSettings()
        {
            ResponseFormat = typeof(PlannerResponse),
        };

        string serviceId = "Ollama__qwen3:4b";
        IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>(serviceKey: serviceId); // TODO: From config
        if (chatService is null)
        {
            throw new InvalidOperationException($"The chat service is not configured. Please ensure that the '{serviceId}' service is registered in the kernel.");
        }
        ChatMessageContent response = await chatService.GetChatMessageContentAsync(messages, kernel: kernel, executionSettings: executionSettings);
        
        string json = response.Content!;
        PlannerResponse? plan = JsonSerializer.Deserialize<PlannerResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        if (plan == null)
        {
            throw new InvalidOperationException($"The planner agent did not return a valid plan. Results: {json}");
        }

        return new PlanCompleteMessage
        {
            History = messages,
            Plan = plan!,
            Response = new ChatResponse(new ChatMessage(ChatRole.Assistant, json))
        };
    }
}