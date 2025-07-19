using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public interface IConsoleChatClient
{
    Task<ChatHistory> ChatAsync(ChatHistory history, CancellationToken cancellationToken = default);
    Task<ChatHistory> ChatIndefinitelyAsync(ChatHistory history, CancellationToken cancellationToken = default);
}