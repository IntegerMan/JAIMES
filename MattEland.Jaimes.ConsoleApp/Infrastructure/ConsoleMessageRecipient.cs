using System.Diagnostics.CodeAnalysis;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using CommunityToolkit.Mvvm.Messaging;
using MattEland.Jaimes.Agents.Messages;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Infrastructure;

public class ConsoleMessageRecipient(IAnsiConsole console) : 
    IRecipient<PlanCompleteMessage>,
    IRecipient<ProcessCreatedMessage>,
    IRecipient<PlanEvaluatedMessage>,
    IRecipient<ResponseComposedMessage>
{
    public void Listen()
    {
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(PlanCompleteMessage message)
    {
        console.Write(new Table()
            .Title("[Cyan]Plan Generated[/]")
            .AddColumns("[Orange3]Key[/]", "[Orange3]Value[/]")
            .AddRow("[Yellow]Checks[/]", message.Plan.Checks)
            .AddRow("[Yellow]Key Points[/]", string.Join(Environment.NewLine, message.Plan.KeyPoints.Select(p => $"- {p}")))
            .AddRow("[Yellow]Cautions[/]", message.Plan.Cautions));
        console.WriteLine();
    }

    public void Receive(PlanEvaluatedMessage message)
    {
        console.DisplayEvaluationResults(message.Evaluation, "Plan Evaluation Results");
    }

    [Experimental("SKEXP0080")]
    public void Receive(ProcessCreatedMessage message)
    {
        console.WriteMermaidNotation(message.Name, message.Process);
    }

    public void Receive(ResponseComposedMessage message)
    {
        console.WriteHistory(message.History);
        
        console.Markup("[Yellow]Composer[/]: ");
        console.WriteLine(message.Response);
    }
}