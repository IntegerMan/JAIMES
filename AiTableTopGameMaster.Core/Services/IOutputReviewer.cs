namespace AiTableTopGameMaster.Core.Services;

/// <summary>
/// Interface for reviewing game master output before presenting it to the player.
/// Provides a safety layer to prevent undesirable behaviors.
/// </summary>
public interface IOutputReviewer
{
    /// <summary>
    /// Reviews the game master's output for undesirable behaviors.
    /// </summary>
    /// <param name="gameMasterOutput">The output from the game master agent</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Review result indicating if the output is acceptable and any feedback</returns>
    Task<OutputReviewResult> ReviewOutputAsync(string gameMasterOutput, CancellationToken cancellationToken = default);
}