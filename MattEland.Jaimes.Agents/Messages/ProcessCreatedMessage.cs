using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Agents.Messages;

#pragma warning disable SKEXP0080
public record ProcessCreatedMessage(string Name, KernelProcess Process);