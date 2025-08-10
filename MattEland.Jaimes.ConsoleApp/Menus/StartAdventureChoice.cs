using AiTableTopGameMaster.ConsoleShared.Clients;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Helpers;
using AiTableTopGameMaster.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Menus;

public class StartAdventureChoice(IServiceProvider services, IAnsiConsole console) : IMenuChoice
{
    public string MenuText => "Start New Adventure";
    public int Order => 1;

    public async Task<ApplicationState> RunAsync()
    {
        Adventure adventure = services.GetRequiredService<Adventure>();
        console.MarkupLine($"{DisplayHelpers.System}Adventure loaded: {adventure.Name} by {adventure.Author}[/]");
        Log.Debug("Adventure loaded: {Name} by {Author}", adventure.Name, adventure.Author);
    
        Character character = services.GetRequiredService<Character>();
        console.MarkupLine($"{DisplayHelpers.System}Playing as: {character.Name} the {character.Specialization}[/]");
        Log.Debug("Character selected: {Name} the {Specialization}", character.Name, character.Specialization);

        console.WriteLine();
        console.MarkupLine("The adventure begins! Type [bold green]'exit'[/] to quit at any time.");
        console.WriteLine();
    
        IPromptsService promptsService = services.GetRequiredService<IPromptsService>();
        ConsoleChatClient client = services.GetRequiredService<ConsoleChatClient>();

        IDictionary<string, object> data = adventure.CreateChatData();
        string message = promptsService.GetInitialGreetingMessage(data);
        await client.ChatIndefinitelyAsync(message, data);

        return ApplicationState.Running;
    }
}