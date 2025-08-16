using Microsoft.Extensions.AI.Evaluation;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Agents.Messages;

public record ResponseEvaluatedMessage(ChatHistory History, string Response, EvaluationResult Evaluation, string StepName);