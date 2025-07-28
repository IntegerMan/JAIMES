using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Plugins.Adventures;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Core.Plugins.Adventures;

public class CharacterInfoPluginTests
{
    [Fact]
    public void GetCharacterSheet_WithPlayerCharacter_ReturnsCharacterSheet()
    {
        // Arrange
        var playerCharacter = new Character
        {
            Name = "Aragorn",
            Specialization = "Ranger",
            CharacterSheet = "Human Ranger Level 5\nHP: 45\nAC: 15\nStr: 16, Dex: 18, Con: 14"
        };

        var adventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "Test locations",
            EncountersOverview = "Test encounters",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            PlayerCharacter = playerCharacter
        };

        var plugin = new CharacterInfoPlugin(adventure);

        // Act
        var result = plugin.GetCharacterSheet();

        // Assert
        result.ShouldBe("Human Ranger Level 5\nHP: 45\nAC: 15\nStr: 16, Dex: 18, Con: 14");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Simple character sheet")]
    [InlineData("Level 1 Fighter\nHP: 10\nAC: 16")]
    public void GetCharacterSheet_WithDifferentCharacterSheets_ReturnsCorrectSheet(string characterSheet)
    {
        // Arrange
        var playerCharacter = new Character
        {
            Name = "Test Character",
            Specialization = "Test Specialization",
            CharacterSheet = characterSheet
        };

        var adventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "Test locations",
            EncountersOverview = "Test encounters",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            PlayerCharacter = playerCharacter
        };

        var plugin = new CharacterInfoPlugin(adventure);

        // Act
        var result = plugin.GetCharacterSheet();

        // Assert
        result.ShouldBe(characterSheet);
    }

    [Fact]
    public void Constructor_WithValidAdventure_InitializesSuccessfully()
    {
        // Arrange
        var adventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "Test locations",
            EncountersOverview = "Test encounters",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            PlayerCharacter = new Character
            {
                Name = "Test",
                Specialization = "Test",
                CharacterSheet = "Test"
            }
        };

        // Act
        var plugin = new CharacterInfoPlugin(adventure);

        // Assert
        plugin.ShouldNotBeNull();
    }
}