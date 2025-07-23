using AiTableTopGameMaster.ConsoleApp.Helpers;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

[UsedImplicitly]
public class ConsoleChatClient(
    Agent gameMasterAgent,
    IAnsiConsole console,
    ILogger<ConsoleChatClient> log)
    : IConsoleChatClient
{
    public async Task<ChatHistory> ChatIndefinitelyAsync(ChatHistory history,
        CancellationToken cancellationToken = default)
    {
        bool needsUserMessage = !history.Any() || history.Last().Role != AuthorRole.User;
        do
        {
            if (!needsUserMessage)
            {
                needsUserMessage = true;
            }
            else
            {
                string? userInput = console.Prompt(new TextPrompt<string?>($"{DisplayHelpers.User}You:[/] ").AllowEmpty());

                if (string.IsNullOrWhiteSpace(userInput) ||
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return history;
                }

                log.LogInformation("User: {UserInput}", userInput);
                history.AddUserMessage(userInput);
            }

            await ChatAsync(history, cancellationToken);
        } while (true);
    }

    public async Task<ChatHistory> ChatAsync(ChatHistory history, CancellationToken cancellationToken = default)
    {
        console.MarkupLine($"{DisplayHelpers.System}Generating response...[/]");
        log.LogDebug("Starting agent chat with {MessageCount} messages in history", history.Count);
        
        try
        {
            // Use the agent framework properly for chatting
            await foreach (var response in gameMasterAgent.InvokeAsync(history, cancellationToken: cancellationToken))
            {
                var message = response.Message;
                history.Add(message);
                
                log.LogInformation("AI: {Content}", message.Content);
                
                console.Markup($"{DisplayHelpers.AI}AI:[/] ");
                console.MarkupLineInterpolated($"{message.Content}");
            }

            console.WriteLine();
            return history;
        }
        catch (Exception ex)
        {
            console.MarkupLineInterpolated($"[dim red]Failed to generate response.[/]");
            log.LogError(ex, "Error during agent chat completion");
            throw;
        }
    }
}