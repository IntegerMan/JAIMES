using System.Diagnostics;
using AiTableTopGameMaster.ConsoleShared.Helpers;
using AiTableTopGameMaster.Core.Cores;
using AiTableTopGameMaster.Core.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleShared.Clients;

public class ConsoleChatClient(
    IAnsiConsole console,
    IEnumerable<AiCore> cores,
    ILoggerFactory loggerFactory)
{
    private readonly AiCore[] _cores = cores.ToArray();
    private readonly ILogger<ConsoleChatClient> _log = loggerFactory.CreateLogger<ConsoleChatClient>();
    public string Name => "Game Master";

    public async Task ChatIndefinitelyAsync(string? userInput, IDictionary<string, object> data)
    {
        ChatHistory history = new();
        do
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                userInput = console.Prompt(new TextPrompt<string?>($"{DisplayHelpers.User}You:[/] ").AllowEmpty());
                if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase)) return;

                _log.LogInformation("User: {UserInput}", userInput);
            }
            else
            {
                userInput = userInput.ResolveVariables(data);
                console.MarkupLine($"{DisplayHelpers.User}You:[/] {userInput}");
                _log.LogDebug("User: {UserInput}", userInput); // This prevents the pre-scripted input from being put in the transcript file
            }

            await ChatAsync(userInput, history, data);

            userInput = null; // Reset user input for next iteration
        } while (true);
    }

    public async Task<ChatResult> ChatAsync(string message, ChatHistory history, IDictionary<string, object> data)
    {
        Stopwatch sw = Stopwatch.StartNew();
        _log.LogInformation("User:\r\n{Message}\r\n", message);
        history.AddUserMessage(message);
        ChatHistory inputHistory = new(history);
        data["UserMessage"] = message;

        ChatResult? reply = null;
        bool isSingleCore = _cores.Length == 1;
        
        foreach (var core in _cores)
        {
            data["InputMessage"] = message;

            do
            {
                _log.LogDebug("Sending {Message} to {Core}", message, core.Name);
                console.Write(new Rule($"{DisplayHelpers.AI}{core.Name}[/] {DisplayHelpers.System}is thinking using {DisplayHelpers.AI}{core.ModelId}[/] and [white]{core.PluginCount} plugins[/]...[/]")
                    .Justify(Justify.Left)
                    .RuleStyle(new Style(foreground: Color.MediumPurple3_1)));

                reply = await core.ChatAsync(message, history, data);
                _log.LogDebug("{Core}: {Content}", core.Name, reply);
                
                if (reply.IsEmpty)
                {
                    console.MarkupLine($"[bold red]The {core.Name} response was empty. Retrying.[/]");
                }
                else if (reply.IsJson)
                {
                    console.MarkupLine($"[bold red]The {core.Name} response appeared to be JSON. Retrying.[/]");
                }
            } while (reply.IsEmpty || reply.IsJson);

            if (core != _cores.Last())
            {
                console.Markup($"{DisplayHelpers.AI}{core.Name}:[/] ");
                console.WriteLine(reply.Message);
                console.WriteLine();
            }

            message = reply.Message;
        }

        sw.Stop();
        
        // This gets logged to the transcript file
        _log.LogInformation("{Agent}:\r\n{Content}\r\n", Name, message);
        history.AddAssistantMessage(message);
        data["LastReply"] = message;
        
        console.Markup($"{DisplayHelpers.AI}{Name}:[/] ");
        console.WriteLine(message);

        return new ChatResult
        {
            Message = message,
            ElapsedMilliseconds = sw.ElapsedMilliseconds,
            Data = data,
            History = isSingleCore ? reply!.History : inputHistory,
            Response = new ChatResponse(new ChatMessage(ChatRole.Assistant, message)),
        };
    }
}