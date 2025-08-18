using System.Text;
using MattEland.Jaimes.Core.Domain;
using MattEland.Jaimes.Core.Helpers;

namespace MattEland.Jaimes.Core.Services;

public class PromptsService(StandardPrompts prompts) : IPromptsService
{
    public string GetInitialGreetingMessage(IDictionary<string, object> data)
    {
        StringBuilder sb = new();
        foreach (var m in prompts.GameStart)
        {
            sb.AppendLine(m.ResolveVariables(data));
        }

        return sb.ToString();
    }
}