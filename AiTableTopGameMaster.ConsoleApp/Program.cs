using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;
using Serilog;

IAnsiConsole console = new LoggingConsoleWrapper(AnsiConsole.Console);

try
{
    console.RenderAigmAppHeader();
    
    Log.Debug("Starting AI Table Top Game Master Console Application");
    ServiceProvider services = ServiceExtensions.BuildServiceProvider(console, args);
    Log.Debug("Services configured successfully");

    Adventure adventure = services.GetRequiredService<Adventure>();
    console.MarkupLine($"{DisplayHelpers.System}Adventure loaded: {adventure.Name} by {adventure.Author}[/]");
    Log.Debug("Adventure loaded: {Name} by {Author}", adventure.Name, adventure.Author);
    
    Character character = services.GetRequiredService<Character>();
    console.MarkupLine($"{DisplayHelpers.System}Playing as: {character.Name} the {character.Specialization}[/]");
    Log.Debug("Character selected: {Name} the {Specialization}", character.Name, character.Specialization);

    console.WriteLine();
    
    ChatHistory history = adventure.StartGame(character);
    Log.Debug("Initial chat history created with {MessageCount} messages", history.Count);
    console.DisplayHistory(history);

    console.WriteLine();

    IAgentChatClient client = services.GetRequiredService<IAgentChatClient>();
    await client.ChatIndefinitelyAsync(history);
    
    console.WriteLine();
    return 0;
} 
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
    console.MarkupLine($"{DisplayHelpers.Error}An error occurred: {ex.Message}[/]");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
