using System.ComponentModel;
using AiTableTopGameMaster.ConsoleApp;
using AiTableTopGameMaster.ConsoleApp.Menus;
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
        // Automatic registration of types by conventions
        services.Scan(scan =>
        {
            // Find all IMainMenuChoice implementations and register them
            scan.FromEntryAssembly()
                .AddClasses(c => c.AssignableTo<IMainMenuChoice>())
                .AsImplementedInterfaces()
                .WithTransientLifetime();
        });
    }, args);
    
    Log.Debug("Services configured successfully");
    ApplicationState state;
    do
    {
        IMainMenuChoice mainMenuChoice = console.Prompt(new SelectionPrompt<IMainMenuChoice>().Title("What do you want to do?")
            .AddChoices(services.GetServices<IMainMenuChoice>()
                .OrderBy(c => c.Order)
                .ThenBy(c => c.MenuText))
            .UseConverter(c => c.MenuText));

        state = await mainMenuChoice.RunAsync();
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
