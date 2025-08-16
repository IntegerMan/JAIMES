using Microsoft.Extensions.AI.Evaluation;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class EvalHelpers
{
    public static void DisplayEvaluationResults(this IAnsiConsole console, EvaluationResult evalResult, string title = "Evaluation Results")
    {
        Table table = new Table().Title(title, new Style(foreground: Color.Blue));
        table.AddColumns("[orange3]Metric[/]", "[orange3]Value[/]", "[orange3]Reason[/]");
        foreach (var kvp in evalResult.Metrics)
        {
            EvaluationMetric metric = kvp.Value;
            string reason = metric.Reason ?? "No Reason Provided";
            string value = metric.ToString() ?? "No Value";
            if (metric is NumericMetric num)
            {
                double? numValue = num.Value;
                if (numValue.HasValue)
                {
                    value = numValue.Value.ToString("F1");
                }
                else
                {
                    value = "No value";
                }
            }

            if (metric.Interpretation is not null)
            {
                reason = metric.Interpretation.Reason ?? reason;
                if (metric.Interpretation.Failed)
                {
                    value = $"[red]{value}[/]";
                    reason = $"[red]{reason}[/]";
                }
                else
                {
                    value = $"[green]{value}[/]";
                }
            }

            table.AddRow(metric.Name, value, reason);
        }

        console.Write(table);
    }
}