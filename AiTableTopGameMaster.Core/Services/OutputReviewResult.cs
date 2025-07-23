namespace AiTableTopGameMaster.Core.Services;

/// <summary>
/// Result of reviewing game master output for undesirable behaviors.
/// </summary>
public class OutputReviewResult
{
    /// <summary>
    /// Whether the output is acceptable to present to the player.
    /// </summary>
    public bool IsAcceptable { get; init; }
    
    /// <summary>
    /// Feedback to provide to the game master if the output needs revision.
    /// Only populated when IsAcceptable is false.
    /// </summary>
    public string? Feedback { get; init; }
    
    /// <summary>
    /// List of specific issues found in the output.
    /// </summary>
    public List<string> Issues { get; init; } = new();
    
    /// <summary>
    /// Creates a result indicating the output is acceptable.
    /// </summary>
    public static OutputReviewResult Acceptable() => new() { IsAcceptable = true };
    
    /// <summary>
    /// Creates a result indicating the output needs revision.
    /// </summary>
    /// <param name="feedback">Feedback for the game master</param>
    /// <param name="issues">Specific issues found</param>
    public static OutputReviewResult NeedsRevision(string feedback, params string[] issues) => new()
    {
        IsAcceptable = false,
        Feedback = feedback,
        Issues = issues.ToList()
    };
}