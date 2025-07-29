using System.Diagnostics;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.ConsoleShared.Infrastructure;
using AiTableTopGameMaster.Core.Models;
using AiTableTopGameMaster.EvaluationConsole;
using AiTableTopGameMaster.EvaluationConsole.Helpers;
using AiTableTopGameMaster.EvaluationConsole.Scenarios;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

IAnsiConsole console = AnsiConsole.Console;

try
{
    console.RenderAppHeader("Core Eval", "Evaluates the performance of different AI cores");
    
    Log.Debug("Starting AI Core Evaluation Console Application");
    ServiceProvider services = ServiceExtensions.BuildServiceProvider<AppSettings>(console, "Adventure", args);
    AppSettings settings = services.GetRequiredService<AppSettings>();
    Log.Debug("Services configured successfully");


    EvaluationScenario scenario = new TestCoreEvaluationScenario(services, settings.ChatModelId);
    ReportingConfiguration reportingConfig = BuildReportingConfig(scenario, services, settings);
    string message = scenario.Message;
    
    for (int i = 1; i <= settings.EvaluationIterations; i++)
    {
        console.MarkupLine($"[bold]Iteration {i}/{settings.EvaluationIterations}[/]");
        console.MarkupLine($"{DisplayHelpers.User}You:[/] {message}");
        console.MarkupLine("\r\n[yellow]Generating a response...[/]");

        Stopwatch stopwatch = Stopwatch.StartNew();
        string response = await scenario.GetResponseAsync(message);
        stopwatch.Stop();
        console.MarkupLine($"[yellow]Response generated in {stopwatch.ElapsedMilliseconds}ms[/]\r\n");
        
        EvaluationResult result = await EvaluateScenario(reportingConfig, scenario, i.ToString(), settings, message, response);
        console.DisplayEvaluationResultsTable(result);
    }
    
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

async Task<EvaluationResult> EvaluateScenario(ReportingConfiguration reportingConfiguration, EvaluationScenario evaluationScenario, string iterationName, AppSettings appSettings, string input, string reply)
{
    await using ScenarioRun run = await reportingConfiguration.CreateScenarioRunAsync(
        evaluationScenario.Name, iterationName);
    console.MarkupLine($"[yellow]Evaluation started using {appSettings.EvaluationModelId}[/]...");

    Stopwatch sw = Stopwatch.StartNew();
    EvaluationResult result = await run.EvaluateAsync(input, reply);
    sw.Stop();
    
    console.MarkupLine($"{DisplayHelpers.Success}Evaluation Complete in {sw.ElapsedMilliseconds}ms[/]");
    
    return result;
}

ReportingConfiguration BuildReportingConfig(EvaluationScenario scenario1, ServiceProvider serviceProvider, AppSettings settings1)
{
    IEnumerable<IEvaluator> evaluators = scenario1.BuildEvaluators();

    ModelFactory modelFactory = serviceProvider.GetRequiredService<ModelFactory>();
    IChatClient chatClient = modelFactory.CreateChatClient(settings1.EvaluationModelId);

    string[] tags = [
        $"EvalModel_{settings1.EvaluationModelId}", 
        $"ChatModel_{settings1.ChatModelId}"
    ];
    ReportingConfiguration reportingConfiguration1 = DiskBasedReportingConfiguration.Create(
        settings1.EvaluationStoragePath,
        evaluators,
        new ChatConfiguration(chatClient),
        enableResponseCaching: true,
        executionName: $"{DateTime.Now:yyyyMMddTHHmmss}",
        tags: tags
    );
    return reportingConfiguration1;
}