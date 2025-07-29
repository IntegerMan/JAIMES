using System.Diagnostics;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.EvaluationConsole.Scenarios;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Spectre.Console;

namespace AiTableTopGameMaster.EvaluationConsole;

public class EvaluationManager(IAnsiConsole console, IChatClient chatClient, AppSettings settings)
{
    public ReportingConfiguration BuildReportingConfig(IEnumerable<IEvaluator> evaluators)
    {
        string[] tags = [
            $"EvalModel_{settings.EvaluationModelId}", 
            $"ChatModel_{settings.ChatModelId}"
        ];
        return DiskBasedReportingConfiguration.Create(
            settings.EvaluationStoragePath,
            evaluators,
            new ChatConfiguration(chatClient),
            enableResponseCaching: true,
            executionName: $"{DateTime.Now:yyyyMMddTHHmmss}",
            tags: tags
        );
    }
    
    public async Task<EvaluationResult> EvaluateScenario(ReportingConfiguration config, EvaluationScenario scenario, string iterationName, AppSettings appSettings, string input, string reply)
    {
        await using ScenarioRun run = await config.CreateScenarioRunAsync(scenario.Name, iterationName, additionalTags: scenario.AdditionalTags);
        
        console.MarkupLine($"[yellow]Evaluation started using {appSettings.EvaluationModelId}[/]...");

        Stopwatch sw = Stopwatch.StartNew();
        EvaluationResult result = await run.EvaluateAsync(input, reply);
        sw.Stop();
    
        console.MarkupLine($"{DisplayHelpers.Success}Evaluation Complete in {sw.ElapsedMilliseconds}ms[/]");
    
        return result;
    }
}