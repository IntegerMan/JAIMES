using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

/// <summary>
/// Interface for agent-based chat client that manages conversations using Semantic Kernel's agents framework.
/// This replaces direct IChatCompletionService usage and enables future multi-agent coordination.
/// </summary>
public interface IAgentChatClient
{
    /// <summary>
    /// Executes a single chat interaction using the agent.
    /// </summary>
    /// <param name="history">The current conversation history</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated chat history with agent response</returns>
    Task<ChatHistory> ChatAsync(ChatHistory history, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts an indefinite chat loop where the user can interact with the agent.
    /// </summary>
    /// <param name="history">Initial conversation history</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Final chat history when session ends</returns>
    Task<ChatHistory> ChatIndefinitelyAsync(ChatHistory history, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the primary game master agent for this session.
    /// Future extension point: This could be enhanced to support multiple agents.
    /// </summary>
    Agent GameMasterAgent { get; }
}