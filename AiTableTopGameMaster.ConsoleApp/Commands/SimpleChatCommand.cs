using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace AiTableTopGameMaster.ConsoleApp.Commands;

public class SimpleChatCommand([FromKeyedServices(ChatClients.Simple)] IConsoleChatClient consoleChat, Adventure adventure) 
    : ChatCommandBase(consoleChat, adventure)
{
    public override string ToString() => "Simple Prompt Engineering";
}