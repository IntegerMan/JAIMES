using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class DisplayHelpers
{
    public static string Instructions => "[bold white]";
    public static string System => "[bold yellow]";
    public static string ToolCall => "[bold orange3]";
    public static string ToolCallResult => "[bold steelblue]";
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
}