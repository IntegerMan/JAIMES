using System.Text;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

/// <summary>
/// Multi-agent chat client that coordinates Planning, Game Master, and Editor agents
/// to provide improved game master responses through a structured pipeline.
/// </summary>
public class MultiAgentChatClient(
    IEnumerable<Agent> agents,
    IAnsiConsole console,
    ILogger<MultiAgentChatClient> log)
    : IConsoleChatClient
{
    private readonly Agent[] _agents = agents.ToArray();
    private readonly ChatHistory _history = new();

    public async Task<ChatHistory> ChatIndefinitelyAsync(string? userMessage, CancellationToken cancellationToken = default)
    {
        do
        {
            userMessage ??= console.Prompt(new TextPrompt<string?>($"{DisplayHelpers.User}You:[/] ").AllowEmpty());

            if (string.IsNullOrWhiteSpace(userMessage) ||
                userMessage.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                return _history;
            }

            log.LogInformation("User: {UserInput}", userMessage);
            _history.AddUserMessage(userMessage);
            userMessage = null;

            await ChatAsync(cancellationToken);
        } while (true);
    }

    public async Task<ChatHistory> ChatAsync(CancellationToken cancellationToken = default)
    {
        console.MarkupLine($"{DisplayHelpers.System}Generating response...[/]");
        log.LogDebug("Starting multi-agent chat with {MessageCount} messages in history", _history.Count);

        try
        {
            ChatHistory passHistory = new(_history);
            string response = passHistory.LastOrDefault()?.Content ?? "Hello";
            foreach (var agent in _agents)
            {
                log.LogDebug("Sending to {AgentName}: {Content}", agent.Name, response);
                console.MarkupLineInterpolated($"[bold blue]{agent.Name} is thinking...[/]");
                
                StringBuilder sbResponse = new();
                await foreach (var r in agent.InvokeAsync(passHistory, cancellationToken: cancellationToken))
                {
                    sbResponse.Append(r.Message.Content);
                }

                response = sbResponse.ToString();
                log.LogInformation("{AgentName}: {Response}", agent.Name, response);
                console.MarkupLineInterpolated($"[bold blue]{agent.Name}:[/] {response}");
                passHistory.AddAssistantMessage(response);
            }
            
            // Only add the final response to the permanent history
            _history.AddAssistantMessage(response);

            return _history;
        }
        catch (Exception ex)
        {
            console.MarkupLineInterpolated($"[dim red]Failed to generate response.[/]");
            log.LogError(ex, "Error during multi-agent chat completion");
            throw;
        }
    }

}