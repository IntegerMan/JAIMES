using AiTableTopGameMaster.ConsoleApp.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Clients;

/// <summary>
/// Multi-agent chat client that coordinates Planning, Game Master, and Editor agents
/// to provide improved game master responses through a structured pipeline.
/// </summary>
public class MultiAgentChatClient : IConsoleChatClient
{
    private readonly Agent _planningAgent;
    private readonly Agent _gameMasterAgent;
    private readonly Agent _editorAgent;
    private readonly IAnsiConsole _console;
    private readonly ILogger<MultiAgentChatClient> _log;

    public MultiAgentChatClient(
        IEnumerable<Agent> agents,
        IAnsiConsole console,
        ILogger<MultiAgentChatClient> log)
    {
        var agentList = agents.ToArray();
        _planningAgent = agentList.First(a => a.Name == "PlanningAgent");
        _gameMasterAgent = agentList.First(a => a.Name == "GameMaster");
        _editorAgent = agentList.First(a => a.Name == "EditorAgent");
        _console = console;
        _log = log;
    }

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
                string? userInput = _console.Prompt(new TextPrompt<string?>($"{DisplayHelpers.User}You:[/] ").AllowEmpty());

                if (string.IsNullOrWhiteSpace(userInput) ||
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    return history;
                }

                _log.LogInformation("User: {UserInput}", userInput);
                history.AddUserMessage(userInput);
            }

            await ChatAsync(history, cancellationToken);
        } while (true);
    }

    public async Task<ChatHistory> ChatAsync(ChatHistory history, CancellationToken cancellationToken = default)
    {
        _console.MarkupLine($"{DisplayHelpers.System}Generating response...[/]");
        _log.LogDebug("Starting multi-agent chat with {MessageCount} messages in history", history.Count);
        
        try
        {
            // Step 1: Planning Agent analyzes the situation and creates a plan
            string plan = await GetPlanningResponse(history, cancellationToken);
            _log.LogInformation("Planning Agent created plan: {Plan}", plan);
            
            // Step 2: Game Master Agent creates response based on the plan
            string gmResponse = await GetGameMasterResponse(history, plan, cancellationToken);
            _log.LogInformation("Game Master Agent created response: {Response}", gmResponse);
            
            // Step 3: Editor Agent improves and proofs the response
            string finalResponse = await GetEditorResponse(gmResponse, cancellationToken);
            _log.LogInformation("Editor Agent improved response: {FinalResponse}", finalResponse);
            
            // Add only the final edited response to the chat history
            var assistantMessage = new ChatMessageContent(AuthorRole.Assistant, finalResponse);
            history.Add(assistantMessage);
            
            // Display the final response to the user
            _console.Markup($"{DisplayHelpers.AI}GameMaster:[/] ");
            _console.MarkupLineInterpolated($"{finalResponse}");
            _console.WriteLine();
            
            return history;
        }
        catch (Exception ex)
        {
            _console.MarkupLineInterpolated($"[dim red]Failed to generate response.[/]");
            _log.LogError(ex, "Error during multi-agent chat completion");
            throw;
        }
    }
    
    private async Task<string> GetPlanningResponse(ChatHistory history, CancellationToken cancellationToken)
    {
        _console.MarkupLine($"{DisplayHelpers.System}Planning response...[/]");
        _log.LogDebug("Planning Agent analyzing situation");
        
        var planningHistory = new ChatHistory();
        
        // Add recent context to planning agent (last few messages for context)
        var recentMessages = history.TakeLast(5);
        foreach (var msg in recentMessages)
        {
            planningHistory.Add(msg);
        }
        
        string plan = "";
        await foreach (var response in _planningAgent.InvokeAsync(planningHistory, cancellationToken: cancellationToken))
        {
            plan += response.Message.Content;
        }
        
        return plan;
    }
    
    private async Task<string> GetGameMasterResponse(ChatHistory history, string plan, CancellationToken cancellationToken)
    {
        _console.MarkupLine($"{DisplayHelpers.System}Creating narrative response...[/]");
        _log.LogDebug("Game Master Agent creating response based on plan");
        
        var gmHistory = new ChatHistory();
        
        // Add recent context to game master agent
        var recentMessages = history.TakeLast(5);
        foreach (var msg in recentMessages)
        {
            gmHistory.Add(msg);
        }
        
        // Add the plan as a system message
        gmHistory.AddSystemMessage($"PLANNING AGENT OUTPUT: {plan}");
        gmHistory.AddUserMessage("Based on the plan above, provide your game master response to the player.");
        
        string response = "";
        await foreach (var gmResponse in _gameMasterAgent.InvokeAsync(gmHistory, cancellationToken: cancellationToken))
        {
            response += gmResponse.Message.Content;
        }
        
        return response;
    }
    
    private async Task<string> GetEditorResponse(string gmResponse, CancellationToken cancellationToken)
    {
        _console.MarkupLine($"{DisplayHelpers.System}Editing and improving response...[/]");
        _log.LogDebug("Editor Agent improving response");
        
        var editorHistory = new ChatHistory();
        editorHistory.AddUserMessage($"Please edit and improve this game master response:\n\n{gmResponse}");
        
        string finalResponse = "";
        await foreach (var editorResponse in _editorAgent.InvokeAsync(editorHistory, cancellationToken: cancellationToken))
        {
            finalResponse += editorResponse.Message.Content;
        }
        
        return finalResponse;
    }
}