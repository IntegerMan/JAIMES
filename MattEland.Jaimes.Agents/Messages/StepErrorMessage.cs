namespace MattEland.Jaimes.Agents.Messages;

public record StepErrorMessage(string StepName, Exception Error);