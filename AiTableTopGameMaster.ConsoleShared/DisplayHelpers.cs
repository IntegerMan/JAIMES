using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleShared;

public static class DisplayHelpers
{
    public static string Instructions => "[bold white]";
    public static string System => "[bold mediumpurple3]";
    public static string AI => "[bold blue]";
    public static string Success => "[bold green]";
    public static string User => "[bold yellow]";
    public static string ToolCall => "[bold slateblue3]";
    public static string ToolCallResult => "[bold lightslategrey]";
    public static string Error => "[bold red]";
    
    public static void RenderAppHeader(this IAnsiConsole console, string name, string description)
    {
        console.Write(new FigletText(name)
            .Justify(Justify.Left)
            .Color(Color.Green));
        
        console.MarkupLine($"{System}by[/] [cyan]Matt Eland[/]");
        console.MarkupLine($"{System}{description}[/]");
        console.WriteLine();
    }

    public static void DisplayHistory(this IAnsiConsole console, ChatHistory history)
    {
        foreach (var message in history)
        {
            if (message.Role == AuthorRole.User)
            {
                console.MarkupLine($"{User}{message.Content}[/]");
            }
            else
            {
                console.MarkupLine($"{ToolCallResult}{message.Content}[/]");
            }
        }
    }

    public static void WaitForKeypress(this IAnsiConsole console)
    {
        console.MarkupLine($"{DisplayHelpers.Error}Press any key to exit...[/]");
        console.Input.ReadKey(intercept: true);
    }
}