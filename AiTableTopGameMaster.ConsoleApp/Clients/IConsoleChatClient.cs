using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public interface IConsoleChatClient
{
    Task<ChatHistory> ChatAsync();
    Task<ChatHistory> ChatIndefinitelyAsync(string? userMessage);
}