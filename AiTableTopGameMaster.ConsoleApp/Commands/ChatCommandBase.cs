using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

public abstract class ChatCommandBase(IConsoleChatClient consoleChat, Adventure adventure) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        ICollection<ChatMessage> history = adventure.GenerateInitialHistory();
        await consoleChat.ChatIndefinitelyAsync(history);

        return 0;
    }

    public abstract override string ToString();
}