namespace MattEland.Jaimes.Agents;

public interface IJaimesAgent<T>
{
    public string Name { get; }
    public string[] Plugins { get; }
}