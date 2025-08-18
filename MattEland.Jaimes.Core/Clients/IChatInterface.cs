namespace MattEland.Jaimes.Core.Clients;

public interface IChatInterface
{
    void DisplayAgentMessage(string messageResponse);
    Task<string> GetPlayerInputAsync();
}