namespace MattEland.Jaimes.Agents.Messages;

public record StepErrorMessage : JaimesMessage
{
    public required string StepName { get; init; }
    public required Exception Error { get; init; }
}