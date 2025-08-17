using System.Diagnostics;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using MattEland.Jaimes.Core.Evaluation;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Menus;

public class EvaluateChoice(IEnumerable<EvaluationScenario> scenarios, IAnsiConsole console) : IMenuChoice
{
    public string MenuText => "Build Evaluation Report";
    public int Order => 5;
    
    public async Task<ApplicationState> RunAsync()
    {
        if (!scenarios.Any())
        {
            throw new InvalidOperationException("No evaluation scenarios found");
        }
        
        foreach (var scenario in scenarios)
        {
            string message = scenario.Message;
    
            console.MarkupLine($"{DisplayHelpers.User}You:[/] {message}");
            console.MarkupLine("\r\n[yellow]Generating a response...[/]");

            Stopwatch stopwatch = Stopwatch.StartNew();
            await scenario.RunAsync(message);
            stopwatch.Stop();
            console.MarkupLine($"[yellow]Response generated in {stopwatch.ElapsedMilliseconds}ms[/]\r\n");
        }
        
        return ApplicationState.Running;
    }

}