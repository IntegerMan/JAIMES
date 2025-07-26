using AiTableTopGameMaster.Core.Services;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Core.Services;

public class AdventureLoaderExtendedTests
{
    private readonly IAdventureLoader _adventureLoader = new AdventureLoader(new NullLoggerFactory());

    [Fact]
    public async Task GetAdventuresAsync_WithNonExistentDirectory_ReturnsEmptyCollection()
    {
        // Arrange
        string nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var adventures = await _adventureLoader.GetAdventuresAsync(nonExistentDir);

        // Assert
        adventures.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAdventuresAsync_WithValidDirectory_LoadsAllValidAdventures()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create multiple valid adventure files
            string adventure1Json = """
                                   {
                                     "name": "Adventure 1",
                                     "author": "Author 1",
                                     "version": "1.0.0",
                                     "ruleset": "D&D 5E",
                                     "backstory": "Backstory 1",
                                     "settingDescription": "Setting 1",
                                     "locationsOverview": "Locations 1",
                                     "locations": [],
                                     "encountersOverview": "Encounters 1",
                                     "encounters": [],
                                     "characters": [],
                                     "gameMasterNotes": "Notes 1",
                                     "narrativeStructure": "Structure 1",
                                     "initialGreetingPrompt": "Greeting 1"
                                   }
                                   """;

            string adventure2Json = """
                                   {
                                     "name": "Adventure 2",
                                     "author": "Author 2",
                                     "version": "2.0.0",
                                     "ruleset": "Pathfinder",
                                     "backstory": "Backstory 2",
                                     "settingDescription": "Setting 2",
                                     "locationsOverview": "Locations 2",
                                     "locations": [],
                                     "encountersOverview": "Encounters 2",
                                     "encounters": [],
                                     "characters": [],
                                     "gameMasterNotes": "Notes 2",
                                     "narrativeStructure": "Structure 2",
                                     "initialGreetingPrompt": "Greeting 2"
                                   }
                                   """;

            await File.WriteAllTextAsync(Path.Combine(tempDir, "adventure1.json"), adventure1Json);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "adventure2.json"), adventure2Json);

            // Act
            var adventures = await _adventureLoader.GetAdventuresAsync(tempDir);

            // Assert
            adventures.ShouldNotBeEmpty();
            adventures.Count().ShouldBe(2);
            
            var adventuresList = adventures.ToList();
            adventuresList.ShouldContain(a => a.Name == "Adventure 1");
            adventuresList.ShouldContain(a => a.Name == "Adventure 2");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GetAdventuresAsync_WithMixedValidAndInvalidFiles_LoadsOnlyValidAdventures()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a valid adventure file
            string validAdventureJson = """
                                       {
                                         "name": "Valid Adventure",
                                         "author": "Valid Author",
                                         "version": "1.0.0",
                                         "ruleset": "D&D 5E",
                                         "backstory": "Valid backstory",
                                         "settingDescription": "Valid setting",
                                         "locationsOverview": "Valid locations",
                                         "locations": [],
                                         "encountersOverview": "Valid encounters",
                                         "encounters": [],
                                         "characters": [],
                                         "gameMasterNotes": "Valid notes",
                                         "narrativeStructure": "Valid structure",
                                         "initialGreetingPrompt": "Valid greeting"
                                       }
                                       """;

            // Create invalid JSON files
            string invalidJson1 = "{ invalid json";
            string invalidJson2 = """
                                 {
                                   "name": "Incomplete Adventure"
                                 }
                                 """;

            await File.WriteAllTextAsync(Path.Combine(tempDir, "valid.json"), validAdventureJson);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "invalid1.json"), invalidJson1);
            await File.WriteAllTextAsync(Path.Combine(tempDir, "invalid2.json"), invalidJson2);
            
            // Create a non-JSON file that should be ignored
            await File.WriteAllTextAsync(Path.Combine(tempDir, "readme.txt"), "This is not a JSON file");

            // Act
            var adventures = await _adventureLoader.GetAdventuresAsync(tempDir);

            // Assert
            adventures.ShouldHaveSingleItem();
            adventures.First().Name.ShouldBe("Valid Adventure");
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GetAdventuresAsync_WithEmptyDirectory_ReturnsEmptyCollection()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var adventures = await _adventureLoader.GetAdventuresAsync(tempDir);

            // Assert
            adventures.ShouldBeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task GetAdventuresAsync_WithDirectoryContainingOnlyNonJsonFiles_ReturnsEmptyCollection()
    {
        // Arrange
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create non-JSON files
            await File.WriteAllTextAsync(Path.Combine(tempDir, "readme.txt"), "This is a readme file");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "config.xml"), "<config></config>");

            // Act
            var adventures = await _adventureLoader.GetAdventuresAsync(tempDir);

            // Assert
            adventures.ShouldBeEmpty();
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task LoadAdventureAsync_WithNullDeserializationResult_ThrowsInvalidOperationException()
    {
        // Arrange
        string tempFile = Path.GetTempFileName();
        
        try
        {
            // Write JSON that would deserialize to null (this is edge case testing)
            await File.WriteAllTextAsync(tempFile, "null");

            // Act & Assert
            await Should.ThrowAsync<InvalidOperationException>(() =>
                _adventureLoader.LoadAdventureAsync(tempFile));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}