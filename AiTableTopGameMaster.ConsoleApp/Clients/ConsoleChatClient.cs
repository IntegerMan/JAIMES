using JetBrains.Annotations;
using Microsoft.Extensions.AI;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;


[UsedImplicitly]
public class ConsoleChatClient(IChatClient chatClient, IAnsiConsole console) : IConsoleChatClient
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
                console.Markup("[blue]You:[/] ");
                string? userInput = console.Ask<string>("Type your message (or 'exit' to quit):");
                if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return history;
                }
            
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
        await console.Status().StartAsync("Generating response", async _ =>
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
        }

        return history;
    }
}