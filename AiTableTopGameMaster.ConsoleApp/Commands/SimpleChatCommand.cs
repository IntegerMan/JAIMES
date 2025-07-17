using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

public class SimpleChatCommand([FromKeyedServices(ChatClients.Simple)] IConsoleChatClient consoleChat, Adventure adventure) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        ICollection<ChatMessage> history = adventure.GenerateInitialHistory();
        await consoleChat.ChatIndefinitelyAsync(history);

        return 0;
    }
}