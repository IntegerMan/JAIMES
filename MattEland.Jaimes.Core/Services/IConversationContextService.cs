namespace MattEland.Jaimes.Core.Services;

public interface IConversationContextService
{
    void SetContext(string key, object?value);
    void SetContext<T>(T? value) where T : class;
    T? GetContext<T>(string key) where T : class;
    T? GetContext<T>() where T : class;
    void ClearContext();
    T GetRequiredContext<T>(string key) where T : class;
}