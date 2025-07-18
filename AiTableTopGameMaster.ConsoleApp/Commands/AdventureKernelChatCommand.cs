using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

[UsedImplicitly]
public class AdventureKernelChatCommand([FromKeyedServices(ChatClients.AdventureKernel)] IConsoleChatClient consoleChat, Adventure adventure) 
    : ChatCommandBase(consoleChat, adventure)
{
    public override string ToString() => "Semantic Kernel with Adventure Functions";
}