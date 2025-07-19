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
    console.MarkupLineInterpolated($"[dim blue]Initializing AI Table Top Game Master...[/]");

    ServiceProvider services = ServiceExtensions.BuildServiceProvider(console, args);
    console.MarkupLineInterpolated($"[dim blue]Services configured successfully.[/]");

    Adventure adventure = services.GetRequiredService<Adventure>();
    console.MarkupLineInterpolated($"[dim blue]Adventure loaded: {adventure.Name} by {adventure.Author}[/]");
    
    ChatHistory history = adventure.GenerateInitialHistory()
                                   .ToChatHistory();
    console.MarkupLineInterpolated($"[dim blue]Chat history initialized with {history.Count} message(s).[/]");

    IConsoleChatClient client =  services.GetRequiredService<IConsoleChatClient>();
    console.MarkupLineInterpolated($"[dim green]Ready for adventure! Type your message below (or 'exit' to quit).[/]");
    console.WriteLine();
    
    await client.ChatIndefinitelyAsync(history);
    
    console.WriteLine();
    return 0;
} 
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
    console.MarkupLine($"[red]An error occurred: {ex.Message}[/]");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
