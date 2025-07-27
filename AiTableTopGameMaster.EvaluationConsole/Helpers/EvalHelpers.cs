using Microsoft.Extensions.AI.Evaluation;
using Spectre.Console;

namespace AiTableTopGameMaster.EvaluationConsole.Helpers;

public static class EvalHelpers
{
    public static void DisplayEvaluationResultsTable(this IAnsiConsole console, EvaluationResult evalResult)
    {
        Table table = new Table().Title("Evaluation Results");
        table.AddColumns("Metric", "Value", "Reason");
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

            table.AddRow(kvp.Key, value, reason);
        }

        console.Write(table);
    }
}