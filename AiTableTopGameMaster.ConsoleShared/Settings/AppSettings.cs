using AiTableTopGameMaster.Core.Cores;
using AiTableTopGameMaster.Core.Settings;

namespace AiTableTopGameMaster.ConsoleShared.Settings;

public class AppSettings
{
    public required OllamaSettings Ollama { get; init; }
    public required string SourcebookPath { get; init; }
    public List<CoreInfo> Cores { get; init; } = [];
}