using AiTableTopGameMaster.Domain;

namespace AiTableTopGameMaster.ConsoleApp.Settings;

public class AppSettings
{
    public required OllamaSettings Ollama { get; init; }
}