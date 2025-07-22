using AiTableTopGameMaster.ConsoleApp.Helpers;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

/// <summary>
/// Agent-based chat client implementation using Semantic Kernel's agents framework.
/// This replaces the direct IChatCompletionService approach with a more structured agent model.
/// </summary>
[UsedImplicitly]
public class AgentChatClient(
    Agent gameMasterAgent,
    IAnsiConsole console,
    ILogger<AgentChatClient> log)
    : IAgentChatClient
{
    public Agent GameMasterAgent { get; } = gameMasterAgent;

    public async Task<ChatHistory> ChatIndefinitelyAsync(ChatHistory history,
        CancellationToken cancellationToken = default)
    {
        bool needsUserMessage = !history.Any() || history.Last().Role != AuthorRole.User;
        do
        {
            if (!needsUserMessage)
            {
                needsUserMessage = true;
            }
            else
            {
                string? userInput = console.Prompt(new TextPrompt<string?>($"{DisplayHelpers.User}You:[/] ").AllowEmpty());

                if (string.IsNullOrWhiteSpace(userInput) ||
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return history;
                }

                log.LogInformation("User: {UserInput}", userInput);
                history.AddUserMessage(userInput);
            }

            await ChatAsync(history, cancellationToken);
        } while (true);
    }

    public async Task<ChatHistory> ChatAsync(ChatHistory history, CancellationToken cancellationToken = default)
    {
        console.MarkupLine($"{DisplayHelpers.System}Generating response...[/]");
        log.LogDebug("Starting agent chat with {MessageCount} messages in history", history.Count);
        
        try
        {
            // EXTENSION POINT: Multi-agent coordination could be added here
            // For example, a conversation could be managed by an AgentGroupChat
            // that coordinates multiple agents (GameMaster, NPC agents, etc.)
            
            // TODO: Future enhancement - Create AgentGroupChat for multi-agent scenarios:
            // var agentGroup = new AgentGroupChat(GameMasterAgent, npcAgent1, npcAgent2);
            // await foreach (var message in agentGroup.InvokeAsync(history))

            // For now, we'll use the kernel from the agent to process the chat
            // This maintains compatibility while using the agent's configuration
            if (GameMasterAgent.Kernel != null)
            {
                var chatService = GameMasterAgent.Kernel.GetRequiredService<IChatCompletionService>();
                var promptSettings = new PromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                };
                
                var response = await chatService.GetChatMessageContentsAsync(
                    history, 
                    promptSettings, 
                    kernel: GameMasterAgent.Kernel,
                    cancellationToken: cancellationToken);

                if (response is { Count: > 0 })
                {
                    console.WriteLine();

                    foreach (ChatMessageContent reply in response)
                    {
                        history.Add(reply);
                        
                        log.LogInformation("AI: {Content}", reply.Content);
                        
                        console.Markup($"{DisplayHelpers.AI}AI:[/] ");
                        console.MarkupLineInterpolated($"{reply.Content}");
                    }
                }
            }

            console.WriteLine();
            return history;
        }
        catch (Exception ex)
        {
            console.MarkupLineInterpolated($"[dim red]Failed to generate response.[/]");
            log.LogError(ex, "Error during agent chat completion");
            throw;
        }
    }
}