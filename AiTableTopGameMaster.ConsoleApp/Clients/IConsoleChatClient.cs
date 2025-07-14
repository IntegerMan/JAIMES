using Microsoft.Extensions.AI;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public interface IConsoleChatClient
{
    Task<IEnumerable<ChatMessage>> ConductConversation(string systemPrompt, CancellationToken cancellationToken = default);
    Task<ICollection<ChatMessage>> ChatAsync(ICollection<ChatMessage> history, CancellationToken cancellationToken = default);
    Task<ICollection<ChatMessage>> ChatAsync(string message, string systemPrompt, CancellationToken cancellationToken = default);
}