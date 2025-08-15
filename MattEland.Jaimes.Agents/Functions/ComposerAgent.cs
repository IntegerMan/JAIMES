using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Helpers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0001

namespace MattEland.Jaimes.Agents.Functions;

public class ComposerAgent(Kernel kernel)
{
    public string Name => "Composer";
    public string[] Plugins => [];
    
    public async Task<ResponseComposedMessage> GenerateAsync(ChatHistory history, PlannerResponse plan)
    {
        ChatHistory agentHistory = [];
        agentHistory.AddSystemMessage("""
                                      You are a game master assistant that generates short responses to the player to convey what's happening in the game.
                                      This should be in response to the game history up to this point and the response plan provided.
                                      Call functions as needed to generate the response.
                                      """);
        
        agentHistory.AddSystemMessage("Based on this history, generate a short response to the player that adheres to the following plan:");
        agentHistory.AddSystemMessage($"Checks Required: {plan.Checks}");
        agentHistory.AddSystemMessage($"Cautions: {plan.Cautions}");
        agentHistory.AddSystemMessage($"Key Points to Include: {Environment.NewLine}{string.Join(Environment.NewLine, plan.KeyPoints.Select(k => $"- {k}"))}");
        
        history.CopyMessagesTo(agentHistory, AuthorRole.User, AuthorRole.Assistant);

        IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();

        ChatMessageContent response = await chatService.GetChatMessageContentAsync(agentHistory, kernel: kernel);

        return new ResponseComposedMessage
        {
            History = agentHistory,
            Response = response.Content ?? "The system did not provide a response."
        };
    }
}