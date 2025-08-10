using System.ComponentModel;
using AiTableTopGameMaster.ConsoleApp;
using AiTableTopGameMaster.ConsoleApp.Evaluation;
using AiTableTopGameMaster.ConsoleApp.Evaluation.Scenarios;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.ConsoleApp.Infrastructure;
using AiTableTopGameMaster.ConsoleApp.Menus;
using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Helpers;
using AiTableTopGameMaster.Core.Models;
using AiTableTopGameMaster.Core.Services;
using Microsoft.Extensions.AI;
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
        // Add an IChatClient for evaluation
        services.AddKeyedSingleton<IChatClient>("Evaluation", (sp, key) =>
        {
            AppSettings settings = sp.GetRequiredService<AppSettings>();
            ModelFactory modelFactory = sp.GetRequiredService<ModelFactory>();
            return modelFactory.CreateChatClient(settings.EvaluationModelId);
        });
        
        // Automatic registration of types by conventions
        services.Scan(scan =>
        {
            services.AddSingleton<EvaluationManager>();
            
            // Find all IMainMenuChoice implementations and register them
            scan.FromEntryAssembly()
                .AddClasses(c => c.AssignableTo<IMenuChoice>())
                .AsImplementedInterfaces()
                .WithTransientLifetime();
            
            // Register all EvaluationScenario implementations
            scan.FromEntryAssembly()
                .AddClasses(c => c.AssignableTo<EvaluationScenario>())
                .As<EvaluationScenario>()
                .WithTransientLifetime();
        });
    }, args);
    
    Log.Debug("Services configured successfully");
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
