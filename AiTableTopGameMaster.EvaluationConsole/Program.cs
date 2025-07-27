using AiTableTopGameMaster.ConsoleShared;
using Serilog;
using Spectre.Console;

IAnsiConsole console = AnsiConsole.Console;

try
{
    console.RenderAppHeader("Core Eval", "Evaluates the performance of different AI cores");

    return 0;
} 
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
    console.MarkupLine($"{DisplayHelpers.Error}An error occurred: {ex.Message}[/]");
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
    return 1;
}
finally
{
    Log.CloseAndFlush();
    console.WaitForKeypress();
}