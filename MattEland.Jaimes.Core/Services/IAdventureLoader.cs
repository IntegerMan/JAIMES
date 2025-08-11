using MattEland.Jaimes.Core.Domain;

namespace MattEland.Jaimes.Core.Services;

public interface IAdventureLoader
{
    Task<Adventure> LoadAdventureAsync(string stringAdventurePath);
    Task<IEnumerable<Adventure>> GetAdventuresAsync(string adventuresDirectory);
}