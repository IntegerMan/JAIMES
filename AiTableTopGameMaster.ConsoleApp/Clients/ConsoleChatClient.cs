using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public class ConsoleChatClient(
    Agent agent,
    IOutputReviewer outputReviewer,
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
            const int maxRevisionAttempts = 3;
            int revisionAttempt = 0;
            
            while (revisionAttempt < maxRevisionAttempts)
            {
                // Generate response from the game master agent
                string gameMasterResponse = await GenerateGameMasterResponseAsync(history, cancellationToken);
                
                // Review the response
                OutputReviewResult reviewResult = await outputReviewer.ReviewOutputAsync(gameMasterResponse, cancellationToken);
                
                if (reviewResult.IsAcceptable)
                {
                    // Response is acceptable, add to history and display to player
                    ChatMessageContent acceptedMessage = new(AuthorRole.Assistant, gameMasterResponse);
                    history.Add(acceptedMessage);
                    
                    log.LogInformation("{Agent}: {Content}", agent.Name, gameMasterResponse);
                    console.Markup($"{DisplayHelpers.AI}{agent.Name}:[/] ");
                    console.MarkupLineInterpolated($"{gameMasterResponse}");
                    console.WriteLine();
                    return history;
                }
                
                // Response needs revision
                revisionAttempt++;
                log.LogDebug("Game master response needs revision (attempt {Attempt}/{Max}): {Issues}", 
                    revisionAttempt, maxRevisionAttempts, string.Join(", ", reviewResult.Issues));
                
                if (revisionAttempt >= maxRevisionAttempts)
                {
                    // Max attempts reached, display the response anyway but with a warning
                    console.MarkupLine($"{DisplayHelpers.System}[yellow]Note: Response may not fully follow game master best practices.[/]");
                    ChatMessageContent finalMessage = new(AuthorRole.Assistant, gameMasterResponse);
                    history.Add(finalMessage);
                    
                    log.LogWarning("Max revision attempts reached, displaying response anyway");
                    console.Markup($"{DisplayHelpers.AI}{agent.Name}:[/] ");
                    console.MarkupLineInterpolated($"{gameMasterResponse}");
                    console.WriteLine();
                    return history;
                }
                
                // Send feedback to the game master for revision
                string revisionPrompt = $"Please revise your previous response. {reviewResult.Feedback}";
                history.AddSystemMessage(revisionPrompt);
                console.MarkupLine($"{DisplayHelpers.System}Refining response...[/]");
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
    
    private async Task<string> GenerateGameMasterResponseAsync(ChatHistory history, CancellationToken cancellationToken)
    {
        string response = "";
        
        await foreach (AgentResponseItem<ChatMessageContent> responseItem in agent.InvokeAsync(history, cancellationToken: cancellationToken))
        {
            response += responseItem.Message.Content;
        }
        
        return response;
    }
}