using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public interface IConsoleChatClient
{
    Task<ChatHistory> ChatAsync(CancellationToken cancellationToken = default);
    Task<ChatHistory> ChatIndefinitelyAsync(string? userMessage, CancellationToken cancellationToken = default);
}