using AiTableTopGameMaster.ConsoleShared.Clients;
using AiTableTopGameMaster.EvaluationConsole.Scenarios;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;

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
    
    public static async Task<EvaluationResult> EvaluateScenario(ReportingConfiguration config, EvaluationScenario scenario, string iterationName, string input, ChatResult reply)
    {
        await using ScenarioRun run = await config.CreateScenarioRunAsync(scenario.Name, iterationName, additionalTags: scenario.AdditionalTags);
        
        IEnumerable<EvaluationContext> context = scenario.BuildContext(reply);
        EvaluationResult result = await run.EvaluateAsync(input, reply.Message, context);
    
        return result;
    }
}