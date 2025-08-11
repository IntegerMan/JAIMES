namespace MattEland.Jaimes.Core.Services;

public interface IPromptsService
{
    string GetInitialGreetingMessage(IDictionary<string, object> data);
}