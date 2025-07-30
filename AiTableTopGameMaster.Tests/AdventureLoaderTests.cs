using AiTableTopGameMaster.Core.Services;
using Shouldly;
using System.Text.Json;
using AiTableTopGameMaster.Core.Domain;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiTableTopGameMaster.Tests;

public class AdventureLoaderTests
{
    private readonly IAdventureLoader _adventureLoader = new AdventureLoader(new NullLoggerFactory());

    [Fact]
    public async Task LoadAdventureAsync_ValidJsonFile_LoadsAdventureCorrectly()
    {
        // Arrange
        string testAdventureJson = """
                                   {
                                     "name": "Test Adventure",
                                     "author": "Test Author",
                                     "version": "1.0.0",
                                     "ruleset": "TestRuleset",
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
                                     "characters": [
                                       {
                                           "name": "TestCharacter",
                                           "specialization": "Level 1 Unit test",
                                           "characterSheet": "Test character sheet"
                                       }
                                     ],
                                     "gameMasterNotes": "Test GM notes",
                                     "narrativeStructure": "Test narrative structure",
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
            adventure.Ruleset.ShouldBe("TestRuleset");
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
            Character character = adventure.Characters.Single();
            character.Name.ShouldBe("TestCharacter");
            character.Specialization.ShouldBe("Level 1 Unit test");
            character.CharacterSheet.ShouldBe("Test character sheet");
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
                                     "ruleset": "TestRuleset",
                                     "backstory": "A test backstory",
                                     "settingDescription": "A test setting",
                                     "locationsOverview": "Test locations overview",
                                     "locations": [],
                                     "encountersOverview": "Test encounters overview",
                                     "encounters": [],
                                   "characters": [
                                     {
                                         "name": "TestCharacter",
                                         "specialization": "Level 1 Unit test",
                                         "characterSheet": "Test character sheet"
                                     }
                                   ],
                                     "gameMasterNotes": "Test GM notes",
                                     "narrativeStructure": "Test narrative structure",
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
            Adventure adventure = await _adventureLoader.LoadAdventureAsync(adventureFile);

            // Assert
            adventure.ShouldNotBeNull();
            adventure.Name.ShouldBe("Directory Test Adventure");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}