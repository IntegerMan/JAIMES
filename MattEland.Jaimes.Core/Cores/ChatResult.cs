using MattEland.Jaimes.Core.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.Core.Cores;

public record ChatResult
{
    public string Message => Response.Text;
    public long ElapsedMilliseconds { get; set; }
    public IDictionary<string, object> Data { get; init; } = new Dictionary<string, object>();
    public required ChatHistory History { get; init; }
    public required ChatResponse Response { get; init; }
    public bool IsJson => Message.IsJson();
    public bool IsEmpty => string.IsNullOrWhiteSpace(Message);
}