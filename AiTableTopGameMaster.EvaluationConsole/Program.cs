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
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Json;
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
        services.AddSingleton<EvaluationManager>();
        services.AddSingleton<IChatClient>(sp =>
        {
            AppSettings settings = sp.GetRequiredService<AppSettings>();
            ModelFactory modelFactory = sp.GetRequiredService<ModelFactory>();
            return modelFactory.CreateChatClient(settings.EvaluationModelId);
        });
    }, args);
    AppSettings settings = services.GetRequiredService<AppSettings>();
    Log.Debug("Services configured successfully");

    EvaluationScenario[] scenarios =
    [
        new PlannerEvaluationScenario(services),
        new AdventureEvaluationScenario(services, "Storyteller"),
        new AdventureEvaluationScenario(services, "Editor"),
        new EndToEndEvaluationScenario(services),
    ];

    EvaluationManager eval = services.GetRequiredService<EvaluationManager>();
    
    IEnumerable<IEvaluator> evaluators =
    [
        new CoherenceEvaluator(),
        //new CompletenessEvaluator(),
        new FluencyEvaluator(),
        //new GroundednessEvaluator(),
        new RelevanceEvaluator(),
        new RelevanceTruthAndCompletenessEvaluator(),
        new StopwatchEvaluator()
        //new EquivalenceEvaluator(),
        //new RetrievalEvaluator()
    ];
    ReportingConfiguration reportingConfig = eval.BuildReportingConfig(evaluators);
    
    foreach (var scenario in scenarios)
    {
        string message = scenario.Message;
    
        for (int i = 1; i <= settings.EvaluationIterations; i++)
        {
            console.MarkupLine($"[bold]{scenario.Name} Iteration {i}/{settings.EvaluationIterations}[/]");
            console.MarkupLine($"{DisplayHelpers.User}You:[/] {message}");
            console.MarkupLine("\r\n[yellow]Generating a response...[/]");

            Stopwatch stopwatch = Stopwatch.StartNew();
            ChatResult response = await scenario.GetResponseAsync(message);
            stopwatch.Stop();
            console.MarkupLine($"[yellow]Response generated in {stopwatch.ElapsedMilliseconds}ms[/]\r\n");
        
            EvaluationResult result = await EvaluationManager.EvaluateScenario(reportingConfig, scenario, i.ToString(), message, response);
            console.DisplayEvaluationResultsTable(result);
        }
    }
    
    console.MarkupLine("\r\n[bold green]Evaluation complete![/]");
    await eval.ExportEvaluationReportAsync(reportingConfig, Environment.CurrentDirectory, openInBrowser: true);
    console.MarkupLine($"Results saved to: {Environment.CurrentDirectory}");

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
