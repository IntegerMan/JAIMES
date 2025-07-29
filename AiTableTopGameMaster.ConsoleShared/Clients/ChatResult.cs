namespace AiTableTopGameMaster.ConsoleShared.Clients;

public record ChatResult
{
    public string Message { get; init; }
    public long EllapsedMilliseconds { get; init; }
    public IDictionary<string, object> Data { get; init; } = new Dictionary<string, object>();
}