using System.Diagnostics;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.ConsoleShared.Infrastructure;
using AiTableTopGameMaster.ConsoleShared.Settings;
using AiTableTopGameMaster.EvaluationConsole.Helpers;
using AiTableTopGameMaster.EvaluationConsole.Scenarios;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

IAnsiConsole console = AnsiConsole.Console;

try
{
    console.RenderAppHeader("Core Eval", "Evaluates the performance of different AI cores");
    
    Log.Debug("Starting AI Core Evaluation Console Application");
    ServiceProvider services = ServiceExtensions.BuildServiceProvider(console, "Adventure", args);
    Log.Debug("Services configured successfully");

    EvaluationScenario scenario = new TestCoreEvaluationScenario(services);
    string message = scenario.Message;
    
    console.MarkupLine($"{DisplayHelpers.User}You:[/] {message}");
    console.MarkupLine("\r\n[yellow]Generating a response...[/]");

    Stopwatch stopwatch = Stopwatch.StartNew();
    string response = await scenario.GetResponseAsync(message);
    stopwatch.Stop();
    console.MarkupLine($"[yellow]Response generated in {stopwatch.ElapsedMilliseconds}ms[/]\r\n");

    AppSettings settings = services.GetRequiredService<AppSettings>();
    console.MarkupLine($"[yellow]Evaluation started using {settings.Ollama.ChatModelId}[/]...");
    
    OllamaChatClient chatClient = new(settings.Ollama.ChatEndpoint, settings.Ollama.ChatModelId); 
    ChatConfiguration config = new(chatClient);
    
    IEvaluator evaluator = new CompositeEvaluator(scenario.BuildEvaluators());
    stopwatch.Restart();
    EvaluationResult result = await evaluator.EvaluateAsync(message, response, config);
    stopwatch.Stop();
    
    console.MarkupLine($"{DisplayHelpers.Success}Evaluation Complete in {stopwatch.ElapsedMilliseconds}ms[/]");
    console.DisplayEvaluationResultsTable(result);
    
    return 0;
} 
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
    console.MarkupLine($"{DisplayHelpers.Error}An error occurred: {ex.Message}[/]");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}
finally
{
    Log.CloseAndFlush();
    console.WaitForKeypress();
}