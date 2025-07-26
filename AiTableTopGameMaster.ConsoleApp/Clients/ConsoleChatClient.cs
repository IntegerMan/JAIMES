using System.Text;
using AiTableTopGameMaster.ConsoleApp.Cores;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public class ConsoleChatClient(
    IAnsiConsole console,
    IEnumerable<AiCore> cores,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<ConsoleChatClient> _log = loggerFactory.CreateLogger<ConsoleChatClient>();

    public async Task ChatIndefinitelyAsync(string? userInput = null)
    {
        ChatHistory history = new();
        do
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                userInput = console.Prompt(new TextPrompt<string?>($"{DisplayHelpers.User}You:[/] ").AllowEmpty());

                if (string.IsNullOrWhiteSpace(userInput) ||
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                _log.LogInformation("User: {UserInput}", userInput);
            }
            else
            {
                console.MarkupLine($"{DisplayHelpers.User}You:[/] {userInput}");
                _log.LogDebug("User: {UserInput}", userInput); // This prevents the pre-scripted input from being put in the transcript file
            }
            await ChatAsync(userInput, history);
            
            userInput = null; // Reset user input for next iteration
        } while (true);
    }

    private async Task ChatAsync(string message, ChatHistory history)
    {
        _log.LogInformation("User:\r\n{Message}\r\n", message);
        history.AddUserMessage(message);
        
        foreach (var core in cores)
        {
            _log.LogDebug("Sending {Message} to {Core}", message, core.Name);
            console.Write(new Rule($"{DisplayHelpers.AI}{core.Name}[/] {DisplayHelpers.System}is thinking...[/]")
                .Justify(Justify.Left)
                .RuleStyle(new Style(foreground: Color.MediumPurple3_1)));

            StringBuilder sb = new();
            await foreach (var reply in core.ChatAsync(message, history))
            {
                _log.LogDebug("{Core}: {Content}", core.Name, reply);
                console.MarkupLine($"{DisplayHelpers.AI}{core.Name}:[/] {reply}");
                sb.Append(reply);
            }

            console.WriteLine();

            message = sb.ToString();
        }
        
        // This gets logged to the transcript file
        _log.LogInformation("Game Master:\r\n{Content}\r\n", message);
        history.AddAssistantMessage(message);
    }
}