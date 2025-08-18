using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using MattEland.Jaimes.Agents.Processes;
using MattEland.Jaimes.Core.Domain;
using MattEland.Jaimes.Core.Helpers;
using MattEland.Jaimes.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
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
    
        
        PipelineRunner runner = services.GetRequiredService<PipelineRunner>();

        IPromptsService promptsService = services.GetRequiredService<IPromptsService>();
        IDictionary<string, object> data = adventure.CreateChatData();
        string message = promptsService.GetInitialGreetingMessage(data);

        ChatHistory history = new();
        history.AddUserMessage(message);
        
        await runner.RunAsync(StandardAdventureProcess.Create(), history, adventure);
        
        // TODO: We'll want to do a conversation loop, either as part of the process or here.

        return ApplicationState.Running;
    }
}