using AiTableTopGameMaster.Domain;

namespace AiTableTopGameMaster.ConsoleApp.Settings;

public class AppSettings
{
    public required OllamaSettings Ollama { get; init; }
    public required string SourcebookPath { get; init; }
    
    /// <summary>
    /// When true, uses the multi-agent approach (Planning → GameMaster → Editor).
    /// When false, uses the legacy single GameMaster agent approach.
    /// </summary>
    public bool UseMultiAgentMode { get; init; } = true;
}