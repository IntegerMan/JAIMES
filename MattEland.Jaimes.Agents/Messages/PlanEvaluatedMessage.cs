using MattEland.Jaimes.Agents.Models;
using Microsoft.Extensions.AI.Evaluation;

namespace MattEland.Jaimes.Agents.Messages;

public record PlanEvaluatedMessage(PlannerResponse Plan, EvaluationResult Evaluation);