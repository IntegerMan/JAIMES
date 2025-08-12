using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Evaluation.Scenarios;

[Experimental("SKEXP0080")]
public class LoggingExternalMessageChannel(IAnsiConsole console) : IExternalKernelProcessMessageChannel
{
    public ValueTask Initialize()
    {
        console.WriteLine("Initializing LoggingExternalMessageChannel...");
        return ValueTask.CompletedTask;
    }

    public ValueTask Uninitialize()
    {
        console.WriteLine("Uninitializing LoggingExternalMessageChannel...");
        return ValueTask.CompletedTask;
    }

    public Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message)
    {
        console.MarkupLine($"[bold yellow]External Event:[/] {externalTopicEvent}");
        console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }
}