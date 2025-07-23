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
                log.LogDebug("Generating response from {AgentName}", agent.Name);
                string gameMasterResponse = "";
                
                try
                {
                    await foreach (AgentResponseItem<ChatMessageContent> responseItem in agent.InvokeAsync(history, cancellationToken: cancellationToken))
                    {
                        if (responseItem.Message?.Content != null)
                        {
                            gameMasterResponse += responseItem.Message.Content;
                        }
                    }
                    
                    log.LogDebug("Generated response length: {Length} characters", gameMasterResponse.Length);
                    
                    if (string.IsNullOrWhiteSpace(gameMasterResponse))
                    {
                        log.LogWarning("Game master agent returned empty or whitespace response");
                        console.MarkupLine($"[red]Warning: Game master returned empty response on attempt {revisionAttempt + 1}[/]");
                        
                        // Treat empty response as needing revision
                        revisionAttempt++;
                        if (revisionAttempt >= maxRevisionAttempts)
                        {
                            console.MarkupLine($"[red]Error: Game master failed to generate response after {maxRevisionAttempts} attempts[/]");
                            return history;
                        }
                        
                        // Add a prompt to encourage the agent to respond
                        history.AddSystemMessage("Please provide a response to continue the game. You must respond with actual content, not an empty message.");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error generating response from game master agent");
                    console.MarkupLine($"[red]Error generating response: {ex.Message}[/]");
                    
                    revisionAttempt++;
                    if (revisionAttempt >= maxRevisionAttempts)
                    {
                        return history;
                    }
                    continue;
                }
                
                // Review the response
                OutputReviewResult reviewResult = await outputReviewer.ReviewOutputAsync(gameMasterResponse, cancellationToken);
                
                if (reviewResult.IsAcceptable || revisionAttempt >= maxRevisionAttempts)
                {
                    // Response is acceptable OR we've reached max attempts
                    if (!reviewResult.IsAcceptable && revisionAttempt >= maxRevisionAttempts)
                    {
                        // Show warning if we're using an unacceptable response due to max attempts
                        console.MarkupLine($"[yellow]Note: Response may not fully follow game master best practices after {maxRevisionAttempts} revision attempts.[/]");
                        log.LogWarning("Max revision attempts reached, displaying latest response");
                    }
                    
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
                log.LogWarning("Game master response needs revision (attempt {Attempt}/{Max}): {Issues}", 
                    revisionAttempt, maxRevisionAttempts, string.Join(", ", reviewResult.Issues));
                
                // Show intermediate response failure for debugging/demo purposes
                console.MarkupLine($"[orange3]Review failed (attempt {revisionAttempt}/{maxRevisionAttempts}): {reviewResult.Feedback}[/]");
                
                // Send feedback to the game master for revision
                string revisionPrompt = $"Please revise your previous response. {reviewResult.Feedback}";
                history.AddSystemMessage(revisionPrompt);
                console.MarkupLine($"{DisplayHelpers.System}Refining response...[/]");
            }

            // Clean up any revision feedback messages from the history before returning
            // Remove any system messages that were added for revision feedback
            var finalHistory = new ChatHistory();
            foreach (var message in history)
            {
                // Only keep non-system messages and original system messages (not revision feedback)
                if (message.Role != AuthorRole.System || 
                    (!message.Content!.Contains("Please revise your previous response")))
                {
                    finalHistory.Add(message);
                }
            }

            console.WriteLine();
            return finalHistory;
        }
        catch (Exception ex)
        {
            console.MarkupLineInterpolated($"[dim red]Failed to generate response.[/]");
            log.LogError(ex, "Error during agent chat completion");
            throw;
        }
    }
}