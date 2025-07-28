using System.Text;
using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Helpers;

namespace AiTableTopGameMaster.Core.Services;

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