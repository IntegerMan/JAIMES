using MattEland.Jaimes.Core.Clients;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public class SpectreConsoleChatClient(IAnsiConsole console) : IChatInterface
{
    public void DisplayAgentMessage(string messageResponse)
    {
        console.Markup("[bold green]Game Master:[/] ");
        console.WriteLine(messageResponse);
        console.WriteLine(); // Add a blank line for better readability
    }

    public Task<string> GetPlayerInputAsync()
    {
        return console.AskAsync<string>("[yellow]You[/]: ");
    }
}