using AiTableTopGameMaster.Core.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.Core.Cores;

public record ChatResult
{
    public required string Message { get; init; }
    public long ElapsedMilliseconds { get; init; }
    public IDictionary<string, object> Data { get; init; } = new Dictionary<string, object>();
    public required ChatHistory History { get; init; }
    public required ChatResponse Response { get; init; }
    public bool IsJson => Message.IsJson();
    public bool IsEmpty => string.IsNullOrWhiteSpace(Message);
}