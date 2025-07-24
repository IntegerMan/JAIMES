using System.Text;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;
#pragma warning disable SKEXP0101
#pragma warning disable SKEXP0001

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

    public async Task<ChatHistory> ChatIndefinitelyAsync(string? userMessage)
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

            await ChatAsync();
        } while (true);
    }

    public async Task<ChatHistory> ChatAsync()
    {
        console.MarkupLine($"{DisplayHelpers.System}Generating response...[/]");
        log.LogDebug("Starting multi-agent chat with {MessageCount} messages in history", _history.Count);

        try
        {
            ChatHistory passHistory = new(_history);
            string response = passHistory.LastOrDefault()?.Content ?? "Hello";
            foreach (var agent in _agents)
            {
                console.MarkupLineInterpolated($"[bold blue]{agent.Name} is thinking...[/]");
                
                ChatHistory agentHistory = new();
                agentHistory.AddSystemMessage(agent.Instructions!);
                foreach (var message in passHistory)
                {
                    log.LogDebug("{AgentName} received {Role} message: {Content}", agent.Name, message.Role, message.Content);
                    if (message.Role == AuthorRole.User)
                    {
                        agentHistory.AddUserMessage(message.Content!);
                    }
                    else if (message.Role == AuthorRole.Assistant)
                    {
                        agentHistory.AddAssistantMessage(message.Content!);
                    }
                }
                
                List<AgentResponseItem<ChatMessageContent>> messages = []; // For debug purposes only
                StringBuilder sbResponse = new();
                AgentInvokeOptions options = new()
                {
                    Kernel = agent.Kernel,
                    AdditionalInstructions = agent.Instructions!,
                    OnIntermediateMessage = (content =>
                    {
                        log.LogDebug("{AgentName} intermediate response: {Content}", agent.Name, content);
                        return Task.CompletedTask;
                    })
                };
                await foreach (var r in agent.InvokeAsync(agentHistory, options: options))
                {
                    messages.Add(r);
                    sbResponse.Append(r.Message.Content);
                }

                // It's possible that the agent didn't return any messages. This case could indicate no changes are needed.
                string agentResponse = sbResponse.ToString().Trim();
                log.LogInformation("{AgentName}: {Response}", agent.Name, agentResponse);
                if (!string.IsNullOrWhiteSpace(agentResponse))
                {
                    if ("{[]}".Contains(agentResponse[0]))
                    {
                        log.LogWarning("{AgentName} returned a response that resembles JSON. Ignoring.", agent.Name);
                        console.MarkupLineInterpolated($"[red]{agent.Name} returned a response that resembles JSON[/]");
                        console.MarkupLineInterpolated($"[yellow]{agentResponse}[/]");
                    }
                    else
                    {
                        response = agentResponse.Trim();
                        console.MarkupLineInterpolated($"[bold blue]{agent.Name}:[/] {response}");
                        passHistory.AddAssistantMessage(response);
                    }
                }

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