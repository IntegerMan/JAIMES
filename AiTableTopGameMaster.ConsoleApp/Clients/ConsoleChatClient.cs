using System.Text;
using AiTableTopGameMaster.ConsoleApp.Cores;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public class ConsoleChatClient(
    IAnsiConsole console,
    IEnumerable<AiCore> cores,
    Adventure adventure,
    Character character,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<ConsoleChatClient> _log = loggerFactory.CreateLogger<ConsoleChatClient>();

    public async Task ChatIndefinitelyAsync(string? userInput = null)
    {
        Dictionary<string, object> data = new()
        {
            ["CharacterName"] = character.Name,
            ["CharacterSpecialization"] = character.Specialization,
            ["AdventureName"] = adventure.Name,
            ["AdventureAuthor"] = adventure.Author,
            ["AdventureBackstory"] = adventure.Backstory,
            ["AdventureSetting"] = adventure.SettingDescription
        };

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

    private async Task ChatAsync(string message, ChatHistory history, Dictionary<string, object> data)
    {
        _log.LogInformation("User:\r\n{Message}\r\n", message);
        history.AddUserMessage(message);
        data["UserMessage"] = message;

        foreach (var core in cores)
        {
            data["InputMessage"] = message;

            string? reply;
            do
            {
                _log.LogDebug("Sending {Message} to {Core}", message, core.Name);
                console.Write(new Rule($"{DisplayHelpers.AI}{core.Name}[/] {DisplayHelpers.System}is thinking...[/]")
                    .Justify(Justify.Left)
                    .RuleStyle(new Style(foreground: Color.MediumPurple3_1)));

                reply = await core.ChatAsync(message, history, data);
                _log.LogDebug("{Core}: {Content}", core.Name, reply);
                
                if (string.IsNullOrWhiteSpace(reply))
                {
                    console.MarkupLine($"[bold red]The {core.Name} response was empty. Retrying.[/]");
                }
                if (reply.IsJson())
                {
                    console.MarkupLine($"[bold red]The {core.Name} response appeared to be JSON. Retrying.[/]");
                }
            } while (string.IsNullOrWhiteSpace(reply) || reply.IsJson());
            
            console.Markup($"{DisplayHelpers.AI}{core.Name}:[/] ");
            console.WriteLine(reply);
            console.WriteLine();

            message = reply;
        }

        // This gets logged to the transcript file
        _log.LogInformation("Game Master:\r\n{Content}\r\n", message);
        history.AddAssistantMessage(message);
        data["LastReply"] = message;
    }
}