using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

[UsedImplicitly]
public class ChatCommand(IConsoleChatClient consoleChat, Adventure adventure) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        ICollection<ChatMessage> history = adventure.GenerateInitialHistory();
        await consoleChat.ChatIndefinitelyAsync(history);

        return 0;
    }
}