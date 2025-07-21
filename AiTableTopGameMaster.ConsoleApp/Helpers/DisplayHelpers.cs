using Microsoft.SemanticKernel.ChatCompletion;
using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

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
    
    public static void RenderAigmAppHeader(this IAnsiConsole console)
    {
        console.Write(new FigletText("AIGM")
            .Justify(Justify.Left)
            .Color(Color.Green));
        
        console.MarkupLine($"{System}by[/] [cyan]Matt Eland[/]");
        console.MarkupLine($"{System}An AI-powered tabletop game master[/]");
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
}