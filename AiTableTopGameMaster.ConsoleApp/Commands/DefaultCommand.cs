using AiTableTopGameMaster.ConsoleApp.Clients;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Spectre.Console;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

[UsedImplicitly]
public class DefaultCommand(IAnsiConsole console, IConsoleChatClient consoleChat) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        ChatMessage message = new(ChatRole.System, "Greet the player and introduce yourself as the AI TableTop Game Master.");
        ChatMessage greeting = new(ChatRole.User, "Hello, I'm the player. Can you introduce yourself?");
        await consoleChat.ChatAsync([message, greeting], cancellationToken: default);

        return 0;
    }
}