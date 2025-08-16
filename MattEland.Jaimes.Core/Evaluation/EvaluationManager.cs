using System.Diagnostics;
using MattEland.Jaimes.Core.Cores;
using MattEland.Jaimes.Core.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Html;
using Microsoft.Extensions.AI.Evaluation.Reporting.Formats.Json;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
#pragma warning disable AIEVAL001

namespace MattEland.Jaimes.Core.Evaluation;

public class EvaluationManager([FromKeyedServices("Evaluation")] IChatClient chatClient)
{
    private ReportingConfiguration? _config;
    public ReportingConfiguration BuildReportingConfig()
    {
        _config = DiskBasedReportingConfiguration.Create(
            Path.Combine(Environment.CurrentDirectory, "Evaluation"),
            evaluators: [
                new CoherenceEvaluator(),
                new FluencyEvaluator(),
                new RelevanceTruthAndCompletenessEvaluator()
            ],
            new ChatConfiguration(chatClient),
            enableResponseCaching: true,
            executionName: $"{DateTime.Now:yyyyMMddTHHmmss}",
            tags: []
        );
        
        return _config;
    }
    
    public async Task<EvaluationResult> EvaluateInteractionAsync(ChatHistory history, ChatResponse reply, string scenario, string iteration = "1")
    {
        ValidateConfig();
        
        await using ScenarioRun run = await _config!.CreateScenarioRunAsync(scenario, iteration);
        
        IEnumerable<ChatMessage> messages = history.Select(m => new ChatMessage(m.Role.ToChatRole(), m.Content));
        EvaluationResult result = await run.EvaluateAsync(messages, reply);
        
        // Customize pass / fail interpretation for metrics to help streamline report interpretation
        result.Interpret(CustomizeMetricInterpretation);
        
        return result;
    }

    private void ValidateConfig()
    {
        if (_config == null)
        {
            throw new InvalidOperationException("Reporting configuration has not been built. Call BuildReportingConfig() first.");
        }
    }

    public Task<EvaluationResult> EvaluateInteractionAsync(ChatHistory history, string reply, string scenario, string iteration = "1")
    {
        ChatMessage message = new(ChatRole.Assistant, reply);
        ChatResponse response = new(message);
        return EvaluateInteractionAsync(history, response, scenario, iteration);
    }
    
    public async Task<EvaluationResult> EvaluateScenarioAsync(EvaluationScenario scenario, string iterationName, ChatResult reply)
    {
        ValidateConfig();
        await using ScenarioRun run = await _config!.CreateScenarioRunAsync(scenario.Name, iterationName, additionalTags: scenario.AdditionalTags);
        
        IEnumerable<EvaluationContext> context = scenario.BuildContext(reply);
        ChatHistory history = reply.History;
        IEnumerable<ChatMessage> messages = history.Select(m => new ChatMessage(m.Role.ToChatRole(), m.Content));
        EvaluationResult result = await run.EvaluateAsync(messages, reply.Response, context);
        
        // Customize pass / fail interpretation for metrics to help streamline report interpretation
        result.Interpret(CustomizeMetricInterpretation);
        
        return result;
    }

    private static EvaluationMetricInterpretation CustomizeMetricInterpretation(EvaluationMetric metric)
    {
        // We like to have many metrics, but we really only want to show red on the high-level report for some metrics
        if (metric is NumericMetric numMetric && IsPassFailMetric(numMetric.Name))
        {
            return new EvaluationMetricInterpretation(numMetric.Interpretation?.Rating ?? EvaluationRating.Unknown, failed: numMetric.Value < 3, reason: $"Completeness metric value: {numMetric.Value}");
        }

        // Do not fail on other metrics
        return new EvaluationMetricInterpretation(metric.Interpretation?.Rating ?? EvaluationRating.Unknown, failed: false, reason: metric.Interpretation?.Reason ?? "No interpretation provided");
    }

    private static bool IsPassFailMetric(string name)
    {
        return name.StartsWith("Completeness") || 
               name.Equals("Equivalence") || 
               name.StartsWith("Relevance");
    }

    public async Task ExportEvaluationReportAsync(string directory, bool openInBrowser = false)
    {
        ValidateConfig();
        
        string reportHtmlPath = Path.Combine(directory, "report.html");
        string reportJsonPath = Path.Combine(directory, "report.json");
        
        HtmlReportWriter htmlWriter = new(reportHtmlPath);
        JsonReportWriter jsonWriter = new(reportJsonPath);
        
        List<ScenarioRunResult> results = new();
        await foreach (string executionName in _config!.ResultStore.GetLatestExecutionNamesAsync(count: 5))
        {
            await foreach (ScenarioRunResult result in _config.ResultStore.ReadResultsAsync(executionName))
            {
                results.Add(result);
            }
        }
        
        await htmlWriter.WriteReportAsync(results, CancellationToken.None);
        await jsonWriter.WriteReportAsync(results, CancellationToken.None);

        if (openInBrowser)
        {
            ProcessStartInfo info = new()
            {
                FileName = reportHtmlPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            };
            Process.Start(info);
        }
    }

}