using AiTableTopGameMaster.Domain;

namespace AiTableTopGameMaster.Core.Services;

public interface IAdventureLoader
{
    Task<Adventure> LoadAdventureAsync(string stringAdventurePath);
    Task<IEnumerable<Adventure>> GetAdventuresAsync(string adventuresDirectory);
}