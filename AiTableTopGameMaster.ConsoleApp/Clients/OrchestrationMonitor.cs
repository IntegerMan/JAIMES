using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Serilog;
using Spectre.Console;
#pragma warning disable SKEXP0001

namespace AiTableTopGameMaster.ConsoleApp.Clients;

public sealed class OrchestrationMonitor(IAnsiConsole console)
{
    public readonly List<StreamingChatMessageContent> StreamedResponses = [];

    public ChatHistory History { get; } = [];

    public ValueTask ResponseCallback(ChatMessageContent response)
    {
        this.History.Add(response);
        console.MarkupLineInterpolated($"[blue]{response.AuthorName}[/]: {response.Content}");
        return ValueTask.CompletedTask;
    }

    public ValueTask StreamingResultCallback(StreamingChatMessageContent streamedResponse, bool isFinal)
    {
        this.StreamedResponses.Add(streamedResponse);

        if (isFinal)
        {
            WriteStreamedResponse(this.StreamedResponses);
            this.StreamedResponses.Clear();
        }

        return ValueTask.CompletedTask;
    }
    
    private void WriteStreamedResponse(IEnumerable<StreamingChatMessageContent> streamedResponses)
    {
        string? authorName = null;
        AuthorRole? authorRole = null;
        StringBuilder builder = new();
        foreach (StreamingChatMessageContent response in streamedResponses)
        {
            authorName ??= response.AuthorName;
            authorRole ??= response.Role;

            if (!string.IsNullOrEmpty(response.Content))
            {
                builder.Append($"({JsonSerializer.Serialize(response.Content)})");
            }
        }

        if (builder.Length > 0)
        {
            string message = builder.ToString();
            Log.Debug("{AuthorName}: {Message}",authorName, message);
            console.MarkupLineInterpolated($"[blue]{authorRole ?? AuthorRole.Assistant}{(authorName is not null ? $" - {authorName}" : string.Empty)}[/]: {message}");
        }
    }
}