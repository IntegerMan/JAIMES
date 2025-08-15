using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Core.Helpers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0001

namespace MattEland.Jaimes.Agents.Functions;

public class EditorAgent(Kernel kernel)
{
    public string Name => "Editor";
    public string[] Plugins => [];
    
    public async Task<ResponseFinalizedMessage> GenerateAsync(ChatHistory history, string draft)
    {
        ChatHistory agentHistory = [];
        agentHistory.AddSystemMessage("""
                                      You are a helping an AI game master deliver a final polished response to the player.
                                      You will take in the play session and history up to this point, along with a draft response.
                                      """);
        
        agentHistory.AddSystemMessage("The game history follows:");
        history.CopyMessagesTo(agentHistory, AuthorRole.Assistant, AuthorRole.User);

        agentHistory.AddSystemMessage("""
                                      Your job is to refine the draft response, ensuring it is clear, concise, and engaging.
                                      Do not add anything to the response that was not in the draft, but you can rephrase, condense, or edit existing content.
                                      Look for problems in the draft response, such as:
                                      - Taking actions on behalf of the player that the player did not explicitly state
                                      - Making assumptions about the player's intent that were not communicated
                                      - Rolling dice instead of asking the player to roll specific checks
                                      """);
        
        agentHistory.AddSystemMessage($"Here is the current draft response: {draft}");
        
        IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
        ChatMessageContent response = await chatService.GetChatMessageContentAsync(agentHistory, kernel: kernel);

        return new ResponseFinalizedMessage
        {
            History = agentHistory,
            Response = response.Content ?? draft,
            Draft = draft
        };
    }
}