using System.Text;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0101
#pragma warning disable SKEXP0001

namespace AiTableTopGameMaster.ConsoleApp.Clients;

/// <summary>
/// Multi-agent chat client that coordinates Planning, Game Master, and Editor agents
/// to provide improved game master responses through a structured pipeline.
/// </summary>
public class MultiAgentChatClient : IConsoleChatClient
{
    private readonly Agent[] _agents;
    private readonly ChatHistory _history = new();
    private readonly IAnsiConsole _console;
    private readonly ILogger<MultiAgentChatClient> _log;
    private readonly InProcessRuntime _runtime;
    private readonly SequentialOrchestration _orchestration;
    private readonly OrchestrationMonitor _monitor;

    /// <summary>
    /// Multi-agent chat client that coordinates Planning, Game Master, and Editor agents
    /// to provide improved game master responses through a structured pipeline.
    /// </summary>
    public MultiAgentChatClient(IEnumerable<Agent> agents,
        IAnsiConsole console,
        ILoggerFactory loggerFactory)
    {
        _console = console;
        _log = loggerFactory.CreateLogger<MultiAgentChatClient>();
        _agents = agents.ToArray();

        _monitor = new OrchestrationMonitor(console);
        _orchestration =
            new SequentialOrchestration(_agents)
            {
                LoggerFactory = loggerFactory,
                ResponseCallback = _monitor.ResponseCallback,
                StreamingResponseCallback = _monitor.StreamingResultCallback
            };

        _runtime = new InProcessRuntime();
    }


    public async Task<ChatHistory> ChatIndefinitelyAsync(string? userMessage)
    {
        await _runtime.StartAsync();
        
        do
        {
            userMessage ??= _console.Prompt(new TextPrompt<string?>($"{DisplayHelpers.User}You:[/] ").AllowEmpty());

            if (string.IsNullOrWhiteSpace(userMessage) ||
                userMessage.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                return _history;
            }

            _log.LogInformation("User: {UserInput}", userMessage);
            _history.AddUserMessage(userMessage);

            //await ChatAsync();

            OrchestrationResult<string> result = await _orchestration.InvokeAsync(userMessage, _runtime);

            string outText = await result.GetValueAsync();
            _log.LogInformation("Orchestration result: {Result}", outText);
            _console.MarkupLineInterpolated($"[bold blue]Game Master:[/] {outText}");
            userMessage = null;
            
            await _runtime.RunUntilIdleAsync();
        } while (true);
    }

    public async Task<ChatHistory> ChatAsync()
    {
        _console.MarkupLine($"{DisplayHelpers.System}Generating response...[/]");
        _log.LogDebug("Starting multi-agent chat with {MessageCount} messages in history", _history.Count);

        try
        {
            ChatHistory passHistory = new(_history);
            string response = passHistory.LastOrDefault()?.Content ?? "Hello";
            foreach (var agent in _agents)
            {
                _console.MarkupLineInterpolated($"[bold blue]{agent.Name} is thinking...[/]");
                
                ChatHistory agentHistory = new();
                agentHistory.AddSystemMessage(agent.Instructions!);
                foreach (var message in passHistory)
                {
                    _log.LogDebug("{AgentName} received {Role} message: {Content}", agent.Name, message.Role, message.Content);
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
                        _log.LogDebug("{AgentName} intermediate response: {Content}", agent.Name, content);
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
                _log.LogInformation("{AgentName}: {Response}", agent.Name, agentResponse);
                if (!string.IsNullOrWhiteSpace(agentResponse))
                {
                    if ("{[]}".Contains(agentResponse[0]))
                    {
                        _log.LogWarning("{AgentName} returned a response that resembles JSON. Ignoring.", agent.Name);
                        _console.MarkupLineInterpolated($"[red]{agent.Name} returned a response that resembles JSON[/]");
                        _console.MarkupLineInterpolated($"[yellow]{agentResponse}[/]");
                    }
                    else
                    {
                        response = agentResponse.Trim();
                        _console.MarkupLineInterpolated($"[bold blue]{agent.Name}:[/] {response}");
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
            _console.MarkupLineInterpolated($"[dim red]Failed to generate response.[/]");
            _log.LogError(ex, "Error during multi-agent chat completion");
            throw;
        }
    }

}