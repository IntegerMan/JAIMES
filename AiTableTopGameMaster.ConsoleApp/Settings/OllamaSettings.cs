namespace AiTableTopGameMaster.ConsoleApp.Settings;

public class OllamaSettings
{
    public required string SystemPrompt { get; init; }
    public required string ChatModelId { get; init; }
    public required string ChatEndpoint { get; init; }
}
