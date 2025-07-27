using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0001

namespace AiTableTopGameMaster.Core.Helpers;

public static partial class ChatExtensions
{
    public static void LogHistory(this ChatHistory history, ILogger log)
    {
        log.LogDebug("Chat History:");
        foreach (var message in history)
        {
            log.LogDebug("{Source}: {Content}", message.AuthorName ?? message.Role.ToString(), message.Content);
        }
    }

    public static bool IsJson(this string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        message = message.Trim();
        
        return (message.StartsWith('{') && message.EndsWith('}')) || (message.StartsWith('[') && message.EndsWith(']'));
    }
    
    /// <summary>
    /// Takes in a string with variable placeholders (e.g. "Hi I'm {{$Name}}") and replaces them with values from the provided data dictionary (e.g. "Hi, I'm Bob").
    /// </summary>
    /// <param name="input">The potentially templated string</param>
    /// <param name="data">The collection of data</param>
    /// <returns>The translated string</returns>
    public static string ResolveVariables(this string input, IDictionary<string, object>? data)
    {
        if (string.IsNullOrWhiteSpace(input) || data == null || data.Count == 0)
        {
            return input;
        }

        return VariableRegex().Replace(input, match =>
        {
            string variable = match.Groups[1].Value;
            if (data.TryGetValue(variable, out object? value))
            {
                return value?.ToString() ?? string.Empty;
            }

            return match.Value;
        });
    }

    [GeneratedRegex(@"\{\{\$(\w+)\}\}")]
    private static partial Regex VariableRegex();
}