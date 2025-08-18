using Microsoft.Extensions.AI.Evaluation;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public record ResponseEvaluatedMessage : JaimesMessage
{
    public required ChatHistory History { get; init; } 
    public required string Response { get; init; }
    public required EvaluationResult Evaluation { get; init; }
    public required string StepName { get; init; }
    public required string ServiceId { get; init; }
}