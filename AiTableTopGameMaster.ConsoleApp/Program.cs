using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Serilog;

IAnsiConsole console = new LoggingConsoleWrapper(AnsiConsole.Console);

try
{
    console.RenderAigmAppHeader();

    ServiceProvider services = ServiceExtensions.BuildServiceProvider(console, args);

    Adventure adventure = services.GetRequiredService<Adventure>();
    ICollection<ChatMessage> history = adventure.GenerateInitialHistory();
    IConsoleChatClient client =  services.GetRequiredService<IConsoleChatClient>();
    
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
