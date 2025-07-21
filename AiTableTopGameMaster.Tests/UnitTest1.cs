using AiTableTopGameMaster.Core.Services;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace AiTableTopGameMaster.Tests;

public class AdventureLoaderTests
{
    private readonly IAdventureLoader _adventureLoader;
    
    public AdventureLoaderTests()
    {
        _adventureLoader = new AdventureLoader();
    }
    
    [Fact]
    public async Task LoadAdventureAsync_ValidJsonFile_LoadsAdventureCorrectly()
    {
        // Arrange
        var testAdventureJson = """
        {
          "name": "Test Adventure",
          "author": "Test Author",
          "version": "1.0.0",
          "backstory": "A test backstory",
          "settingDescription": "A test setting",
          "locationsOverview": "Test locations overview",
          "locations": [
            {
              "name": "Test Location",
              "description": "A test location description"
            }
          ],
          "encountersOverview": "Test encounters overview",
          "encounters": [
            {
              "name": "Test Encounter",
              "description": "A test encounter description"
            }
          ],
          "gameMasterNotes": "Test GM notes",
          "narrativeStructure": "Test narrative structure",
          "characterSheet": "Test character sheet",
          "gameMasterSystemPrompt": "Test system prompt",
          "initialGreetingPrompt": "Test greeting prompt"
        }
        """;
        
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, testAdventureJson);
        
        try
        {
            // Act
            var adventure = await _adventureLoader.LoadAdventureAsync(tempFile);
            
            // Assert
            Assert.NotNull(adventure);
            Assert.Equal("Test Adventure", adventure.Name);
            Assert.Equal("Test Author", adventure.Author);
            Assert.Equal("1.0.0", adventure.Version);
            Assert.Equal("A test backstory", adventure.Backstory);
            Assert.Equal("A test setting", adventure.SettingDescription);
            Assert.Equal("Test locations overview", adventure.LocationsOverview);
            Assert.Single(adventure.Locations);
            Assert.Equal("Test Location", adventure.Locations.First().Name);
            Assert.Equal("Test encounters overview", adventure.EncountersOverview);
            Assert.Single(adventure.Encounters);
            Assert.Equal("Test Encounter", adventure.Encounters.First().Name);
            Assert.Equal("Test GM notes", adventure.GameMasterNotes);
            Assert.Equal("Test narrative structure", adventure.NarrativeStructure);
            Assert.Equal("Test character sheet", adventure.CharacterSheet);
            Assert.Equal("Test system prompt", adventure.GameMasterSystemPrompt);
            Assert.Equal("Test greeting prompt", adventure.InitialGreetingPrompt);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
    
    [Fact]
    public async Task LoadAdventureAsync_FileNotFound_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "non-existent-file.json");
        
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            _adventureLoader.LoadAdventureAsync(nonExistentFile));
    }
    
    [Fact]
    public async Task LoadAdventureAsync_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, invalidJson);
        
        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => 
                _adventureLoader.LoadAdventureAsync(tempFile));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
    
    [Fact]
    public async Task LoadAdventureAsync_MissingRequiredProperty_ThrowsJsonException()
    {
        // Arrange
        var incompleteJson = """
        {
          "name": "Test Adventure"
        }
        """;
        
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, incompleteJson);
        
        try
        {
            // Act & Assert
            await Assert.ThrowsAsync<JsonException>(() => 
                _adventureLoader.LoadAdventureAsync(tempFile));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
    
    [Fact]
    public async Task LoadAdventureAsync_WithDirectoryAndName_LoadsCorrectly()
    {
        // Arrange
        var testAdventureJson = """
        {
          "name": "Directory Test Adventure",
          "author": "Test Author",
          "version": "1.0.0",
          "backstory": "A test backstory",
          "settingDescription": "A test setting",
          "locationsOverview": "Test locations overview",
          "locations": [],
          "encountersOverview": "Test encounters overview",
          "encounters": [],
          "gameMasterNotes": "Test GM notes",
          "narrativeStructure": "Test narrative structure",
          "characterSheet": "Test character sheet",
          "gameMasterSystemPrompt": "Test system prompt",
          "initialGreetingPrompt": "Test greeting prompt"
        }
        """;
        
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var adventureFile = Path.Combine(tempDir, "test-adventure.json");
        await File.WriteAllTextAsync(adventureFile, testAdventureJson);
        
        try
        {
            // Act
            var adventure = await _adventureLoader.LoadAdventureAsync("test-adventure", tempDir);
            
            // Assert
            Assert.NotNull(adventure);
            Assert.Equal("Directory Test Adventure", adventure.Name);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
    
    [Fact]
    public void GenerateInitialHistory_CreatesCorrectChatHistory()
    {
        // Arrange
        var adventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author", 
            Version = "1.0.0",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "Test locations",
            EncountersOverview = "Test encounters",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            CharacterSheet = "Test character",
            GameMasterSystemPrompt = "Test system prompt",
            InitialGreetingPrompt = "Test greeting"
        };
        
        // Act
        var history = adventure.GenerateInitialHistory();
        
        // Assert
        Assert.NotNull(history);
        Assert.Equal(5, history.Count);
        
        var messages = history.ToArray();
        Assert.Equal(ChatRole.System, messages[0].Role);
        Assert.Contains("Test system prompt", messages[0].Text);
        
        Assert.Equal(ChatRole.Tool, messages[1].Role);
        Assert.Contains("Test backstory", messages[1].Text);
        
        Assert.Equal(ChatRole.Tool, messages[2].Role);
        Assert.Contains("Test setting", messages[2].Text);
        
        Assert.Equal(ChatRole.Tool, messages[3].Role);
        Assert.Contains("level 1 rogue", messages[3].Text);
        
        Assert.Equal(ChatRole.User, messages[4].Role);
        Assert.Contains("Test greeting", messages[4].Text);
    }
}
