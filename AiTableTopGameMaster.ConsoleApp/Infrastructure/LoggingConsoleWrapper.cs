using System.Text;
using Serilog;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace AiTableTopGameMaster.ConsoleApp.Infrastructure;

public class LoggingConsoleWrapper(IAnsiConsole console) : IAnsiConsole
{
    public void Clear(bool home) => console.Clear(home);
    public void Write(IRenderable renderable)
    {
        console.Write(renderable);

        switch (renderable)
        {
            case Paragraph p:
            {
                StringBuilder sb = new();
                foreach (var segment in p.GetSegments(console))
                {
                    sb.Append(segment.Text);
                }
                Log.Information(sb.ToString());
                break;
            }
            case Text _:
                // This is typically ignorable and represents an empty line. We can't easily get at the internal text.
                break;
            case ControlCode _:
                // This is ignorable and is used for animations
                break;
            default:
                Log.Debug("Unhandled Renderable: {Renderable}", renderable.ToString());
                break;
        }
    }

    public Profile Profile => console.Profile;

    public IAnsiConsoleCursor Cursor => console.Cursor;

    public IAnsiConsoleInput Input => console.Input;

    public IExclusivityMode ExclusivityMode => console.ExclusivityMode;

    public RenderPipeline Pipeline => console.Pipeline;
}