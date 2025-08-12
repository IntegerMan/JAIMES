using AiTableTopGameMaster.ConsoleApp;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using AiTableTopGameMaster.ConsoleApp.Menus;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Serilog;

IAnsiConsole console = new LoggingConsoleWrapper(AnsiConsole.Console);

try
{
    console.RenderAppHeader("JAIMES", "Join AI to Make Epic Stories");
    
    Log.Debug("Starting AI Table Top Game Master Console Application");
    
    IServiceProvider services = ServiceExtensions.BuildServiceProvider<AppSettings>(console, "Adventure", args);
    
    Log.Debug("Services configured successfully");
    
    // This object will listen for event messages and display them as they occur
    ConsoleMessageRecipient listener = new(console);
    listener.Listen();
    
    ApplicationState state;
    do
    {
        IMenuChoice menuChoice = console.Prompt(new SelectionPrompt<IMenuChoice>().Title("What do you want to do?")
            .AddChoices(services.GetServices<IMenuChoice>()
                .OrderBy(c => c.Order)
                .ThenBy(c => c.MenuText))
            .UseConverter(c => c.MenuText));

        state = await menuChoice.RunAsync();
        console.WriteLine();
    } while (state != ApplicationState.Terminating);

    console.WriteLine("Application terminated normally.");
    return 0;
} 
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    console.WriteLine("Application terminated abnormally.");
    return 1;
}
finally
{
    Log.CloseAndFlush();
    console.WaitForKeypress();
}
