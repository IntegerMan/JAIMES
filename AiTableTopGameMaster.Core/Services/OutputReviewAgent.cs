using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.Core.Services;

/// <summary>
/// Agent-based implementation of output reviewer that uses AI to detect undesirable behaviors
/// in game master output.
/// </summary>
public class OutputReviewAgent : IOutputReviewer
{
    private readonly Agent _reviewAgent;
    private readonly ILogger<OutputReviewAgent> _logger;
    
    private const string ReviewSystemPrompt = """
        You are a specialized AI assistant that reviews game master responses in tabletop RPGs to ensure they follow proper game master etiquette and don't overstep boundaries.

        Your job is to analyze game master outputs and identify any undesirable behaviors, specifically:

        1. ROLLING DICE FOR THE PLAYER: The game master should never roll dice on behalf of the player or decide dice results for them. The player should always be asked to roll their own dice.

        2. ASKING PLAYER ABOUT CHARACTER MEMORIES: The game master should not ask the player what their character remembers from past events. The GM controls the narrative and should tell the player what their character remembers or knows.

        3. TAKING ACTIONS FOR THE PLAYER: The game master should not decide what actions the player's character takes without the player's explicit direction. Phrases like "you decide to...", "you enter and...", "you climb..." are problematic unless the player specifically stated they wanted to do those actions.

        4. MAKING DECISIONS FOR THE PLAYER: The GM should not make choices or decisions on behalf of the player character.

        REVIEW PROCESS:
        - If the output contains NONE of these issues, respond with exactly: "ACCEPTABLE"
        - If the output contains ANY of these issues, respond with exactly: "NEEDS_REVISION" followed by specific feedback

        When providing feedback, be specific about what needs to be changed and suggest how to rephrase the response to be more appropriate.

        Examples of problematic phrases to watch for:
        - "Roll a d20 for..." (should ask player to roll)
        - "You rolled a 15..." (should not assume dice results)
        - "What does your character remember about..." (should tell player what character remembers)
        - "You decide to climb the tree..." (should ask what player wants to do)
        - "You enter the room and see..." (should ask if player wants to enter first)

        Be thorough but concise in your analysis.
        """;
    
    public OutputReviewAgent(Kernel kernel, ILogger<OutputReviewAgent> logger)
    {
        _logger = logger;
        
        _reviewAgent = new ChatCompletionAgent
        {
            Name = "OutputReviewer",
            Description = "Reviews game master output for undesirable behaviors",
            Instructions = ReviewSystemPrompt,
            Kernel = kernel
        };
        
        _logger.LogDebug("OutputReviewAgent initialized");
    }
    
    public async Task<OutputReviewResult> ReviewOutputAsync(string gameMasterOutput, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gameMasterOutput))
        {
            _logger.LogDebug("Empty or whitespace game master output provided");
            return OutputReviewResult.Acceptable();
        }
        
        _logger.LogDebug("Reviewing game master output: {Output}", gameMasterOutput);
        
        try
        {
            // Create a chat history for the review
            ChatHistory reviewHistory = new();
            reviewHistory.AddUserMessage($"Please review this game master response:\n\n{gameMasterOutput}");
            
            // Get the review response
            string reviewResponse = "";
            await foreach (var response in _reviewAgent.InvokeAsync(reviewHistory, cancellationToken: cancellationToken))
            {
                reviewResponse += response.Message.Content;
            }
            
            _logger.LogDebug("Review agent response: {Response}", reviewResponse);
            
            // Parse the review response
            return ParseReviewResponse(reviewResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during output review");
            // On error, default to accepting the output to avoid blocking gameplay
            return OutputReviewResult.Acceptable();
        }
    }
    
    private OutputReviewResult ParseReviewResponse(string reviewResponse)
    {
        if (string.IsNullOrWhiteSpace(reviewResponse))
        {
            _logger.LogWarning("Empty review response from agent");
            return OutputReviewResult.Acceptable();
        }
        
        reviewResponse = reviewResponse.Trim();
        
        if (reviewResponse.StartsWith("ACCEPTABLE", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Output review result: Acceptable");
            return OutputReviewResult.Acceptable();
        }
        
        if (reviewResponse.StartsWith("NEEDS_REVISION", StringComparison.OrdinalIgnoreCase))
        {
            // Extract feedback after "NEEDS_REVISION"
            string feedback = reviewResponse.Substring("NEEDS_REVISION".Length).Trim();
            
            _logger.LogDebug("Output review result: Needs revision - {Feedback}", feedback);
            
            return OutputReviewResult.NeedsRevision(
                feedback.Length > 0 ? feedback : "The response contains undesirable behaviors and needs revision.",
                "Output flagged by review agent"
            );
        }
        
        // If response doesn't match expected format, log warning and default to acceptable
        _logger.LogWarning("Unexpected review response format: {Response}", reviewResponse);
        return OutputReviewResult.Acceptable();
    }
}