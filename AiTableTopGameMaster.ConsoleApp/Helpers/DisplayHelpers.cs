using Spectre.Console;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class DisplayHelpers
{
    public static void RenderAigmAppHeader(this IAnsiConsole console)
    {
        console.Write(new FigletText("AIGM")
            .Justify(Justify.Left)
            .Color(Color.Green));
        
        console.MarkupLine("[blue]by[/] [cyan]Matt Eland[/]");
        console.MarkupLine("[blue]An AI-powered tabletop game master[/]");
    }
}