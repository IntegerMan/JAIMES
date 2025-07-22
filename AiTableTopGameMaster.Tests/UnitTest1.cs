using AiTableTopGameMaster.Core.Services;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;
using Shouldly;
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
        string testAdventureJson = """
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
          "characters": [],
          "gameMasterNotes": "Test GM notes",
          "narrativeStructure": "Test narrative structure",
          "characterSheet": "Test character sheet",
          "gameMasterSystemPrompt": "Test system prompt",
          "initialGreetingPrompt": "Test greeting prompt"
        }
        """;
        
        string tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, testAdventureJson);
        
        try
        {
            // Act
            Adventure adventure = await _adventureLoader.LoadAdventureAsync(tempFile);
            
            // Assert
            adventure.ShouldNotBeNull();
            adventure.Name.ShouldBe("Test Adventure");
            adventure.Author.ShouldBe("Test Author");
            adventure.Version.ShouldBe("1.0.0");
            adventure.Backstory.ShouldBe("A test backstory");
            adventure.SettingDescription.ShouldBe("A test setting");
            adventure.LocationsOverview.ShouldBe("Test locations overview");
            adventure.Locations.ShouldHaveSingleItem();
            adventure.Locations.First().Name.ShouldBe("Test Location");
            adventure.EncountersOverview.ShouldBe("Test encounters overview");
            adventure.Encounters.ShouldHaveSingleItem();
            adventure.Encounters.First().Name.ShouldBe("Test Encounter");
            adventure.GameMasterNotes.ShouldBe("Test GM notes");
            adventure.NarrativeStructure.ShouldBe("Test narrative structure");
            adventure.CharacterSheet.ShouldBe("Test character sheet");
            adventure.GameMasterSystemPrompt.ShouldBe("Test system prompt");
            adventure.InitialGreetingPrompt.ShouldBe("Test greeting prompt");
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
        string nonExistentFile = Path.Combine(Path.GetTempPath(), "non-existent-file.json");
        
        // Act & Assert
        await Should.ThrowAsync<FileNotFoundException>(() => 
            _adventureLoader.LoadAdventureAsync(nonExistentFile));
    }
    
    [Fact]
    public async Task LoadAdventureAsync_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        string invalidJson = "{ invalid json";
        string tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, invalidJson);
        
        try
        {
            // Act & Assert
            await Should.ThrowAsync<JsonException>(() => 
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
        string incompleteJson = """
        {
          "name": "Test Adventure"
        }
        """;
        
        string tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, incompleteJson);
        
        try
        {
            // Act & Assert
            await Should.ThrowAsync<JsonException>(() => 
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
        string testAdventureJson = """
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
          "characters": [],
          "gameMasterNotes": "Test GM notes",
          "narrativeStructure": "Test narrative structure",
          "characterSheet": "Test character sheet",
          "gameMasterSystemPrompt": "Test system prompt",
          "initialGreetingPrompt": "Test greeting prompt"
        }
        """;
        
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        string adventureFile = Path.Combine(tempDir, "test-adventure.json");
        await File.WriteAllTextAsync(adventureFile, testAdventureJson);
        
        try
        {
            // Act
            Adventure adventure = await _adventureLoader.LoadAdventureAsync("test-adventure", tempDir);
            
            // Assert
            adventure.ShouldNotBeNull();
            adventure.Name.ShouldBe("Directory Test Adventure");
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
        Adventure adventure = new Adventure
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
            InitialGreetingPrompt = "Test greeting",
            Characters = 
            [
                new Character
                {
                    Name = "TestCharacter",
                    Description = "A test character",
                    Level = "level 1",
                    Class = "fighter",
                    IsPlayerCharacter = true
                }
            ]
        };
        
        // Act
        ICollection<ChatMessage> history = adventure.GenerateInitialHistory();
        
        // Assert
        history.ShouldNotBeNull();
        history.Count.ShouldBe(5);
        
        ChatMessage[] messages = history.ToArray();
        messages[0].Role.ShouldBe(ChatRole.System);
        messages[0].Text.ShouldContain("Test system prompt");
        
        messages[1].Role.ShouldBe(ChatRole.Tool);
        messages[1].Text.ShouldContain("Test backstory");
        
        messages[2].Role.ShouldBe(ChatRole.Tool);
        messages[2].Text.ShouldContain("Test setting");
        
        messages[3].Role.ShouldBe(ChatRole.Tool);
        messages[3].Text.ShouldContain("TestCharacter");
        messages[3].Text.ShouldContain("level 1");
        messages[3].Text.ShouldContain("fighter");
        
        messages[4].Role.ShouldBe(ChatRole.User);
        messages[4].Text.ShouldContain("Test greeting");
    }
}
