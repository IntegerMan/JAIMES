using Microsoft.Extensions.AI;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

public class DefaultCommand(IAnsiConsole console, AppSettings settings, IChatClient chat) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        await console.Status().StartAsync("Generating greeting", async _ =>
        {
            ChatMessage message = new(ChatRole.System,
                "Greet the player and introduce yourself as the AI TableTop Game Master.");
            ChatMessage greeting = new(ChatRole.User, "Hello, I'm the player. Can you introduce yourself?");

            ChatResponse response = await chat.GetResponseAsync([message, greeting], cancellationToken: default);

            console.MarkupLineInterpolated($"[green]AI Response:[/] {response.Text}");
        });

        return 0;
    }
}