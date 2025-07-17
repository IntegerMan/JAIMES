using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

[UsedImplicitly]
public class ConsoleChatClient(IChatClient chatClient, IAnsiConsole console, ILogger<ConsoleChatClient> log) : IConsoleChatClient
{
    public Task<IEnumerable<ChatMessage>> ChatIndefinitelyAsync(string systemPrompt, CancellationToken cancellationToken = default)
    {
        List<ChatMessage> history = [new(ChatRole.System, systemPrompt)];
        return ChatIndefinitelyAsync(history, cancellationToken);
    }

    public async Task<IEnumerable<ChatMessage>> ChatIndefinitelyAsync(ICollection<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        bool needsUserMessage = !history.Any() || history.Last().Role != ChatRole.User;
        do
        {
            if (needsUserMessage)
            {
                string? userInput = console.Prompt(new TextPrompt<string?>("[blue]You:[/] ").AllowEmpty());
                
                if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return history;
                }
            
                log.LogInformation("User: {UserInput}", userInput);
                history.Add(new ChatMessage(ChatRole.User, userInput));
            }
            else
            {
                needsUserMessage = true;
            }
            await ChatAsync(history, cancellationToken);
        } while (true);
    }    
    public Task<ICollection<ChatMessage>> ChatAsync(string message, string systemPrompt, CancellationToken cancellationToken = default) 
        => ChatAsync((List<ChatMessage>)[
            new ChatMessage(ChatRole.System, systemPrompt),
            new ChatMessage(ChatRole.User, message)
        ], cancellationToken);

    public async Task<ICollection<ChatMessage>> ChatAsync(ICollection<ChatMessage> history, CancellationToken cancellationToken = default)
    {
        ChatResponse? response = null;
        await console.Status().StartAsync("Generating...", async _ =>
        {
            response = await chatClient.GetResponseAsync(history, cancellationToken: cancellationToken);
        });
            
        if (response == null)
        {
            throw new InvalidOperationException("The chat client did not return a response.");
        }

        if (!response.Messages.Any())
        {
            throw new InvalidOperationException("The chat client returned an empty response.");
        }

        console.WriteLine();
        
        foreach (ChatMessage reply in response.Messages)
        {
            history.Add(reply);
            console.MarkupLineInterpolated($"[green]AI:[/] {reply.Text}");
            log.LogInformation("AI: {Text}", reply.Text);
        }

        console.WriteLine();

        return history;
    }
}