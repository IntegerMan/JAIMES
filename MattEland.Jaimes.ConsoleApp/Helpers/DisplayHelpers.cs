using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Process.Tools;
using Spectre.Console;
using Spectre.Console.Json;

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
    
    public static string? WriteAsJson(this IAnsiConsole console, object? obj)
    {
        if (obj is null)
        {
            console.MarkupLine("[dim gray]null[/]");
            return null;
        }

        string json = JsonSerializer.Serialize(obj);
        console.Write(new JsonText(json));
        console.WriteLine();
        return json;
    }

    [Experimental("SKEXP0080")]
    public static void WriteMermaidNotation(this IAnsiConsole console, ProcessBuilder process)
    {
        console.WriteMermaidNotation(process.Name, process.Build());
    }
    
    [Experimental("SKEXP0080")]
    public static void WriteMermaidNotation(this IAnsiConsole console, string name, KernelProcess process)
    {
        string mermaidGraph = process.ToMermaid(maxLevel: 5);
        console.Write(new Panel(new Text(mermaidGraph))
            .Header($"{name} Process Graph", Justify.Center)
            .NoBorder()
            .Expand()
            .BorderColor(Color.Aqua));
    }

    public static void WriteHistory(this IAnsiConsole console, ChatHistory history)
    {
        foreach (var message in history)
        {
            if (message.Role == AuthorRole.User)
            {
                console.Markup($"{User}User: [/]");
            }
            else if (message.Role == AuthorRole.Assistant)
            {
                console.Markup($"{AI}AI[/]: ");
            }
            else
            {
                console.Markup($"[dim]{message.Role}[/]: ");
            }
            console.WriteLine(message.Content ?? "");
        }
    }
}