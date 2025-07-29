using AiTableTopGameMaster.ConsoleApp;
using AiTableTopGameMaster.ConsoleShared.Clients;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.ConsoleShared.Infrastructure;
using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Helpers;
using AiTableTopGameMaster.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Serilog;

IAnsiConsole console = new LoggingConsoleWrapper(AnsiConsole.Console);

try
{
    console.RenderAppHeader("JAIMES", "Join AI to Make Epic Stories");
    
    Log.Debug("Starting AI Table Top Game Master Console Application");
    ServiceProvider services = ServiceExtensions.BuildServiceProvider<AppSettings>(console, "Adventure", services =>
    {
        
    }, args);
    Log.Debug("Services configured successfully");

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
    
    console.WriteLine();
    return 0;
} 
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}
finally
{
    Log.CloseAndFlush();
    console.WaitForKeypress();
}
