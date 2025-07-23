using System.Text.Json;
using System.Text.Json.Serialization;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.Logging;

namespace AiTableTopGameMaster.Core.Services;

public class AdventureLoader(ILoggerFactory loggerFactory) : IAdventureLoader
{
    private readonly ILogger<AdventureLoader> _logger = loggerFactory.CreateLogger<AdventureLoader>();
    
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<Adventure> LoadAdventureAsync(string adventurePath)
    {
        string jsonContent = await File.ReadAllTextAsync(adventurePath);
        
        _logger.LogDebug("Loading adventure from: {AdventurePath}", adventurePath);
        Adventure? adventure = JsonSerializer.Deserialize<Adventure>(jsonContent, _jsonOptions);
        
        if (adventure is null)
        {
            _logger.LogError("Failed to deserialize adventure from: {AdventurePath}. The file may be corrupted, have invalid JSON format, or be missing required properties.", adventurePath);
            throw new InvalidOperationException($"Failed to deserialize adventure from: {adventurePath}. The file may be corrupted, have invalid JSON format, or be missing required properties.");
        }
        
        _logger.LogDebug("Adventure '{AdventureName}' loaded successfully from {AdventurePath}", adventure.Name, adventurePath);
        return adventure;
    }

    
    public async Task<IEnumerable<Adventure>> GetAdventuresAsync(string adventuresDirectory)
    {
        if (!Directory.Exists(adventuresDirectory))
        {
            _logger.LogWarning("Adventure directory does not exist: {AdventuresDirectory}", adventuresDirectory);
            return [];
        }
        
        string[] jsonFiles = Directory.GetFiles(adventuresDirectory, "*.json");
        List<Adventure> adventures = [];
        
        foreach (string file in jsonFiles)
        {
            try
            {
                adventures.Add(await LoadAdventureAsync(file));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load adventure from file: {FilePath}. It may be invalid or corrupted.", file);
                // Skip invalid adventure files
            }
        }

        return adventures;
    }
}