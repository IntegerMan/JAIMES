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
    console.RenderJaimesAppHeader();
    
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
    console.MarkupLine("The adventure begins! Type [bold green]'exit'[/] to quit at any time.");
    console.WriteLine();
    
    ConsoleChatClient client = services.GetRequiredService<ConsoleChatClient>();
    await client.ChatIndefinitelyAsync(adventure.InitialGreetingPrompt);
    
    console.WriteLine();
    return 0;
} 
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
    console.MarkupLine($"{DisplayHelpers.Error}An error occurred: {ex.Message}[/]");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    
    // Wait for user to acknowledge error
    console.MarkupLine($"{DisplayHelpers.Error}Press any key to exit...[/]");
    console.Input.ReadKey(intercept: true);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
