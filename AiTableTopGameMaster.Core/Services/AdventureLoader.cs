using System.Text.Json;
using System.Text.Json.Serialization;
using AiTableTopGameMaster.Domain;

namespace AiTableTopGameMaster.Core.Services;

public interface IAdventureLoader
{
    Task<Adventure> LoadAdventureAsync(string adventurePath);
    Task<Adventure> LoadAdventureAsync(string adventureName, string adventuresDirectory);
    Task<IEnumerable<string>> GetAvailableAdventuresAsync(string adventuresDirectory);
}

public class AdventureLoader : IAdventureLoader
{
    private readonly JsonSerializerOptions _jsonOptions;
    
    public AdventureLoader()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }
    
    public async Task<Adventure> LoadAdventureAsync(string adventurePath)
    {
        if (!File.Exists(adventurePath))
        {
            throw new FileNotFoundException($"Adventure file not found: {adventurePath}");
        }
        
        var jsonContent = await File.ReadAllTextAsync(adventurePath);
        
        var adventure = JsonSerializer.Deserialize<Adventure>(jsonContent, _jsonOptions);
        
        if (adventure == null)
        {
            throw new InvalidOperationException($"Failed to deserialize adventure from: {adventurePath}");
        }
        
        return adventure;
    }
    
    public async Task<Adventure> LoadAdventureAsync(string adventureName, string adventuresDirectory)
    {
        var adventurePath = Path.Combine(adventuresDirectory, $"{adventureName}.json");
        return await LoadAdventureAsync(adventurePath);
    }
    
    public async Task<IEnumerable<string>> GetAvailableAdventuresAsync(string adventuresDirectory)
    {
        if (!Directory.Exists(adventuresDirectory))
        {
            return Enumerable.Empty<string>();
        }
        
        var jsonFiles = Directory.GetFiles(adventuresDirectory, "*.json");
        var adventures = new List<string>();
        
        foreach (var file in jsonFiles)
        {
            try
            {
                var adventure = await LoadAdventureAsync(file);
                adventures.Add(adventure.Name);
            }
            catch
            {
                // Skip invalid adventure files
            }
        }
        
        return adventures;
    }
}