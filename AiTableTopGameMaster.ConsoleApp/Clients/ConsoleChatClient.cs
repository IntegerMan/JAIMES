using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

[UsedImplicitly]
public class ConsoleChatClient(
    PromptExecutionSettings settings,
    IAnsiConsole console,
    Kernel kernel,
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
                string? userInput = console.Prompt(new TextPrompt<string?>("[blue]You:[/] ").AllowEmpty());

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
        console.MarkupLineInterpolated($"[yellow]Generating response...[/]");
        
        IChatCompletionService chatService = kernel.GetRequiredService<IChatCompletionService>();
        IReadOnlyList<ChatMessageContent>? response = await chatService.GetChatMessageContentsAsync(history, settings, kernel: kernel,
            cancellationToken: cancellationToken);

        console.MarkupLineInterpolated($"[dim yellow]Response generated.[/]");

        if (response is not { Count: > 0 })
        {
            throw new InvalidOperationException("The chat client did not return any responses.");
        }

        console.WriteLine();

        foreach (var reply in response)
        {
            history.Add(reply);
            console.MarkupLineInterpolated($"[green]AI:[/] {reply.Content}");
            log.LogInformation("AI: {Content}", reply.Content);
        }

        console.WriteLine();
        return history;
    }
}