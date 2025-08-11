using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using AiTableTopGameMaster.ConsoleApp.Evaluation;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using MattEland.Jaimes.Core.Cores;
using MattEland.Jaimes.Core.Evaluation;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Menus;

public class EvaluateChoice(IEnumerable<EvaluationScenario> scenarios,
    EvaluationManager eval,
    AppSettings settings,
    IAnsiConsole console) : IMenuChoice
{
    public string MenuText => "Build Evaluation Report";
    public int Order => 5;
    
    [Experimental("AIEVAL001")]
    public async Task<ApplicationState> RunAsync()
    {
        IEnumerable<IEvaluator> evaluators =
        [
            new CoherenceEvaluator(),
            new FluencyEvaluator(),
            new RelevanceEvaluator(),
            new RelevanceTruthAndCompletenessEvaluator(),
            new CompletenessEvaluator(), // Note: better coverage from the RelevanceTruthAndCompletenessEvaluator. May be redundant.
            new StopwatchEvaluator(),
            new EquivalenceEvaluator(),
            //new ToolCallAccuracyEvaluator(),
            //new TaskAdherenceEvaluator()
            //new GroundednessEvaluator(),
            //new RetrievalEvaluator()
        ];
        
        ReportingConfiguration reportingConfig = eval.BuildReportingConfig(evaluators);

        if (!scenarios.Any())
        {
            throw new InvalidOperationException("No evaluation scenarios found");
        }
        
        foreach (var scenario in scenarios)
        {
            string message = scenario.Message;
    
            foreach (var modelId in settings.ModelIdsToEvaluate)
            {
                console.MarkupLine($"[bold]{scenario.Name}:{modelId}[/]");
                console.MarkupLine($"{DisplayHelpers.User}You:[/] {message}");
                console.MarkupLine("\r\n[yellow]Generating a response...[/]");

                Stopwatch stopwatch = Stopwatch.StartNew();
                ChatResult response = await scenario.GetResponseAsync(message, modelId);
                stopwatch.Stop();
                response.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                console.MarkupLine($"[yellow]Response generated in {stopwatch.ElapsedMilliseconds}ms[/]\r\n");
        
                EvaluationResult result = await EvaluationManager.EvaluateScenarioAsync(reportingConfig, scenario, modelId, response);
                console.DisplayEvaluationResultsTable(result);
            }
        }
    
        console.MarkupLine("\r\n[bold green]Evaluation complete![/]");
        await eval.ExportEvaluationReportAsync(reportingConfig, Environment.CurrentDirectory, openInBrowser: true);
        console.MarkupLine($"Results saved to: {Environment.CurrentDirectory}");
        
        return ApplicationState.Running;
    }

}