using Serilog;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Infrastructure;

public class LoggingConsoleInputWrapper(IAnsiConsoleInput console) : IAnsiConsoleInput
{
    public bool IsKeyAvailable()
    {
        return console.IsKeyAvailable();
    }

    public ConsoleKeyInfo? ReadKey(bool intercept) 
        => LogKey(console.ReadKey(intercept));

    private static ConsoleKeyInfo? LogKey(ConsoleKeyInfo? consoleKeyInfo)
    {
        if (consoleKeyInfo.HasValue)
        {
            Log.Verbose("Key pressed: {Key}, Modifiers: {Modifiers}, KeyChar: {KeyChar}",
                consoleKeyInfo.Value.Key,
                consoleKeyInfo.Value.Modifiers,
                consoleKeyInfo.Value.KeyChar);
        }

        return consoleKeyInfo;
    }

    public async Task<ConsoleKeyInfo?> ReadKeyAsync(bool intercept, CancellationToken cancellationToken) 
        => LogKey(await console.ReadKeyAsync(intercept, cancellationToken));
}