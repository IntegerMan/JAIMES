using AiTableTopGameMaster.Core.Domain;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Domain;

public class AdventureTests
{
    [Fact]
    public void Adventure_Initialization_SetsPropertiesCorrectly()
    {
        // Arrange
        var testCharacter = new Character 
        { 
            Name = "Test Hero", 
            Specialization = "Warrior", 
            CharacterSheet = "Level 1 Fighter" 
        };
        
        var testLocation = new Location 
        { 
            Name = "Test Village", 
            Description = "A peaceful village" 
        };
        
        var testEncounter = new Encounter 
        { 
            Name = "Goblin Attack", 
            Description = "A sudden ambush" 
        };

        // Act
        var adventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "A long time ago...",
            SettingDescription = "A fantasy world",
            LocationsOverview = "Various locations",
            EncountersOverview = "Dangerous encounters",
            GameMasterNotes = "Important notes",
            NarrativeStructure = "Three act structure",
            Characters = [testCharacter],
            Locations = [testLocation],
            Encounters = [testEncounter],
            PlayerCharacter = testCharacter
        };

        // Assert
        adventure.Name.ShouldBe("Test Adventure");
        adventure.Author.ShouldBe("Test Author");
        adventure.Version.ShouldBe("1.0.0");
        adventure.Ruleset.ShouldBe("D&D 5E");
        adventure.Backstory.ShouldBe("A long time ago...");
        adventure.SettingDescription.ShouldBe("A fantasy world");
        adventure.LocationsOverview.ShouldBe("Various locations");
        adventure.EncountersOverview.ShouldBe("Dangerous encounters");
        adventure.GameMasterNotes.ShouldBe("Important notes");
        adventure.NarrativeStructure.ShouldBe("Three act structure");
        adventure.Characters.ShouldHaveSingleItem();
        adventure.Locations.ShouldHaveSingleItem();
        adventure.Encounters.ShouldHaveSingleItem();
        adventure.PlayerCharacter.ShouldBe(testCharacter);
    }

    [Fact]
    public void Adventure_DefaultCollections_AreInitializedEmpty()
    {
        // Arrange & Act
        var adventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author", 
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "A long time ago...",
            SettingDescription = "A fantasy world",
            LocationsOverview = "Various locations",
            EncountersOverview = "Dangerous encounters",
            GameMasterNotes = "Important notes",
            NarrativeStructure = "Three act structure"
        };

        // Assert
        adventure.Characters.ShouldBeEmpty();
        adventure.Locations.ShouldBeEmpty();
        adventure.Encounters.ShouldBeEmpty();
        adventure.PlayerCharacter.ShouldBeNull();
    }

    [Fact]
    public void Adventure_RulesetProperty_CanBeModified()
    {
        // Arrange
        var adventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0", 
            Ruleset = "D&D 5E",
            Backstory = "A long time ago...",
            SettingDescription = "A fantasy world",
            LocationsOverview = "Various locations",
            EncountersOverview = "Dangerous encounters",
            GameMasterNotes = "Important notes",
            NarrativeStructure = "Three act structure"
        };

        // Act
        adventure.Ruleset = "Pathfinder";

        // Assert
        adventure.Ruleset.ShouldBe("Pathfinder");
    }

    [Fact]
    public void Adventure_PlayerCharacterProperty_CanBeModified()
    {
        // Arrange
        var initialCharacter = new Character 
        { 
            Name = "Initial Hero", 
            Specialization = "Warrior", 
            CharacterSheet = "Level 1 Fighter" 
        };
        
        var newCharacter = new Character 
        { 
            Name = "New Hero", 
            Specialization = "Mage", 
            CharacterSheet = "Level 1 Wizard" 
        };

        var adventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "A long time ago...",
            SettingDescription = "A fantasy world",
            LocationsOverview = "Various locations",
            EncountersOverview = "Dangerous encounters",
            GameMasterNotes = "Important notes",
            NarrativeStructure = "Three act structure",
            PlayerCharacter = initialCharacter
        };

        // Act
        adventure.PlayerCharacter = newCharacter;

        // Assert
        adventure.PlayerCharacter.ShouldBe(newCharacter);
        adventure.PlayerCharacter.Name.ShouldBe("New Hero");
    }
}