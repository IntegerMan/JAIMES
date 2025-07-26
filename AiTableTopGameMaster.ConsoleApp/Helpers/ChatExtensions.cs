using Microsoft.SemanticKernel.ChatCompletion;
using Serilog;
#pragma warning disable SKEXP0001

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class ChatExtensions
{
    public static void LogHistory(this ChatHistory history)
    {
        Log.Debug("Chat History:");
        foreach (var message in history)
        {
            Log.Information("{Source}: {Content}", message.AuthorName ?? message.Role.ToString(), message.Content);
        }
    }
}