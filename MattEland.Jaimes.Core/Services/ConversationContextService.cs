using System.Collections.Concurrent;

namespace MattEland.Jaimes.Core.Services;

public class ConversationContextService : IConversationContextService
{
    private readonly IDictionary<string, object?> _context = new ConcurrentDictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    
    public void SetContext(string key, object?value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (value == null)
        {
            _context.Remove(key);
        }
        else
        {
            _context[key] = value;
        }
    }

    public void SetContext<T>(T? value) where T : class
    {
        SetContext("TYPE__" + typeof(T).Name, value);
    }

    public T? GetContext<T>(string key) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        if (_context.TryGetValue(key, out object? value) && value is T typedValue)
        {
            return typedValue;
        }
        
        return null;
    }

    public T? GetContext<T>() where T : class
    {
        return GetContext<T>("TYPE__" + typeof(T).Name);
    }

    public void ClearContext()
    {
        _context.Clear();
    }

    public T GetRequiredContext<T>(string key) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        if (!_context.TryGetValue(key, out object? value))
        {
            throw new KeyNotFoundException($"Context for key '{key}' not found.");
        }
        return value as T ?? throw new InvalidCastException($"Context for key '{key}' is not of type {typeof(T).Name}.");
    }
}