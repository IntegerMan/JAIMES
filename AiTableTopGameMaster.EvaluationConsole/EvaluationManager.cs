using System.Diagnostics;
using AiTableTopGameMaster.Core.Cores;
using AiTableTopGameMaster.Core.Helpers;
using AiTableTopGameMaster.EvaluationConsole.Scenarios;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Json;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.EvaluationConsole;

public class EvaluationManager(IChatClient chatClient, AppSettings settings)
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
    
    public static async Task<EvaluationResult> EvaluateScenario(ReportingConfiguration config, EvaluationScenario scenario, string iterationName, ChatResult reply)
    {
        await using ScenarioRun run = await config.CreateScenarioRunAsync(scenario.Name, iterationName, additionalTags: scenario.AdditionalTags);
        
        IEnumerable<EvaluationContext> context = scenario.BuildContext(reply);
        ChatHistory history = reply.History;
        IEnumerable<ChatMessage> messages = history.Select(m => new ChatMessage(m.Role.ToChatRole(), m.Content));
        EvaluationResult result = await run.EvaluateAsync(messages, reply.Response, context);
        
        // Customize pass / fail interpretation for metrics to help streamline report interpretation
        result.Interpret(metric =>
        {
            // We like to have many metrics, but we really only want to show red on the high-level report for some metrics
            if (metric is NumericMetric numMetric && IsPassFailMetric(numMetric.Name))
            {
                return new EvaluationMetricInterpretation(
                    numMetric.Interpretation?.Rating ?? EvaluationRating.Unknown,
                    failed: numMetric.Value < 3,
                    reason: $"Completeness metric value: {numMetric.Value}"
                );
            }

            // Do not fail on other metrics
            return new EvaluationMetricInterpretation(
                metric.Interpretation?.Rating ?? EvaluationRating.Unknown,
                failed: false,
                reason: metric.Interpretation?.Reason ?? "No interpretation provided"
            );
        });
        
        return result;
    }

    private static bool IsPassFailMetric(string name)
    {
        return name.StartsWith("Completeness") || 
               name.Equals("Equivalence") || 
               name.StartsWith("Relevance");
    }

    public async Task ExportEvaluationReportAsync(ReportingConfiguration reportingConfiguration, string directory, bool openInBrowser = false)
    {
        string reportHtmlPath = Path.Combine(directory, "report.html");
        string reportJsonPath = Path.Combine(directory, "report.json");
        
        HtmlReportWriter htmlWriter = new(reportHtmlPath);
        JsonReportWriter jsonWriter = new(reportJsonPath);
        
        List<ScenarioRunResult> results = new();
        await foreach (string executionName in reportingConfiguration.ResultStore.GetLatestExecutionNamesAsync(count: 5))
        {
            await foreach (ScenarioRunResult result in reportingConfiguration.ResultStore.ReadResultsAsync(executionName))
            {
                results.Add(result);
            }
        }
        
        await htmlWriter.WriteReportAsync(results, CancellationToken.None);
        await jsonWriter.WriteReportAsync(results, CancellationToken.None);

        if (openInBrowser)
        {
            Process.Start( new ProcessStartInfo
                {
                    FileName = reportHtmlPath,
                    UseShellExecute = true
                });
        }
    }

}