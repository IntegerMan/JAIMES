using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

[UsedImplicitly]
public class EmptySemanticKernelChatCommand([FromKeyedServices(ChatClients.SimpleSemanticKernel)] IConsoleChatClient consoleChat, Adventure adventure) : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        ICollection<ChatMessage> history = adventure.GenerateInitialHistory();
        await consoleChat.ChatIndefinitelyAsync(history);

        return 0;
    }
    
    public override string ToString() => "Simple Functionless Semantic Kernel";

}