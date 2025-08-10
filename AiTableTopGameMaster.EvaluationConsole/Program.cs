using System.Diagnostics;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.ConsoleShared.Infrastructure;
using AiTableTopGameMaster.Core.Cores;
using AiTableTopGameMaster.Core.Models;
using AiTableTopGameMaster.EvaluationConsole;
using AiTableTopGameMaster.EvaluationConsole.Evaluators;
using AiTableTopGameMaster.EvaluationConsole.Helpers;
using AiTableTopGameMaster.EvaluationConsole.Scenarios;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;
#pragma warning disable AIEVAL001

IAnsiConsole console = AnsiConsole.Console;

try
{
    console.RenderAppHeader("Core Eval", "Evaluates the performance of different AI cores");
    
    Log.Debug("Starting AI Core Evaluation Console Application");
    ServiceProvider services = ServiceExtensions.BuildServiceProvider<AppSettings>(console, "Adventure", services =>
    {
        services.AddSingleton<IChatClient>(sp =>
        {
            AppSettings settings = sp.GetRequiredService<AppSettings>();
            ModelFactory modelFactory = sp.GetRequiredService<ModelFactory>();
            return modelFactory.CreateChatClient(settings.EvaluationModelId);
        });
    }, args);
    AppSettings settings = services.GetRequiredService<AppSettings>();
    Log.Debug("Services configured successfully");

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
