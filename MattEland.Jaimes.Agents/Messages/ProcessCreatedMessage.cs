using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Messages;

#pragma warning disable SKEXP0080
public record ProcessCreatedMessage : JaimesMessage
{
    public required string Name { get; init; }
    public required KernelProcess Process { get; init; } 
}