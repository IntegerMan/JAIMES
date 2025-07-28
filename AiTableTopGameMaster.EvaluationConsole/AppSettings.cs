using AiTableTopGameMaster.Core;
using AiTableTopGameMaster.Core.Models;

namespace AiTableTopGameMaster.EvaluationConsole;

public class AppSettings : ISettingsRoot
{
    public required string ChatModelId { get; init; }
    public required string EvaluationModelId { get; init; }
    public AzureOpenAIModelSettings AzureOpenAI { get; init; } = new();
}