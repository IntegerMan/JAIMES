using AiTableTopGameMaster.Core.Plugins.Adventures;
using AiTableTopGameMaster.Domain;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Core.Plugins.Adventures;

public class EncountersPluginTests
{
    private readonly Adventure _adventureWithEncounters;
    private readonly Adventure _adventureWithoutEncounters;

    public EncountersPluginTests()
    {
        _adventureWithEncounters = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "Test locations",
            EncountersOverview = "This adventure includes several challenging encounters:",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            InitialGreetingPrompt = "Test greeting",
            Encounters = 
            [
                new Encounter { Name = "Goblin Ambush", Description = "A group of goblins attacks from the bushes." },
                new Encounter { Name = "Dragon Lair", Description = "An ancient red dragon guards its treasure hoard." },
                new Encounter { Name = "Riddle Challenge", Description = "A sphinx poses three challenging riddles." }
            ]
        };

        _adventureWithoutEncounters = new Adventure
        {
            Name = "Simple Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "Test locations",
            EncountersOverview = "This is a roleplay-focused adventure with no pre-scripted encounters.",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            InitialGreetingPrompt = "Test greeting"
        };
    }

    [Fact]
    public void ListAllEncounters_WithEncounters_ReturnsOverviewAndList()
    {
        // Arrange
        var plugin = new EncountersPlugin(_adventureWithEncounters);

        // Act
        var result = plugin.ListAllEncounters();

        // Assert
        result.ShouldContain("This adventure includes several challenging encounters:");
        result.ShouldContain("The following encounters have been pre-scripted for the adventure:");
        result.ShouldContain("- Goblin Ambush");
        result.ShouldContain("- Dragon Lair");
        result.ShouldContain("- Riddle Challenge");
        result.ShouldContain("You can use the DescribeEncounter command");
    }

    [Fact]
    public void ListAllEncounters_WithoutEncounters_ReturnsOnlyOverview()
    {
        // Arrange
        var plugin = new EncountersPlugin(_adventureWithoutEncounters);

        // Act
        var result = plugin.ListAllEncounters();

        // Assert
        result.ShouldContain("This is a roleplay-focused adventure with no pre-scripted encounters.");
        result.ShouldNotContain("The following encounters have been pre-scripted for the adventure:");
        result.ShouldNotContain("You can use the DescribeEncounter command");
    }

    [Theory]
    [InlineData("Goblin Ambush", "A group of goblins attacks from the bushes.")]
    [InlineData("Dragon Lair", "An ancient red dragon guards its treasure hoard.")]
    [InlineData("Riddle Challenge", "A sphinx poses three challenging riddles.")]
    public void DescribeEncounter_WithValidEncounterName_ReturnsEncounterDetails(string encounterName, string expectedDescription)
    {
        // Arrange
        var plugin = new EncountersPlugin(_adventureWithEncounters);

        // Act
        var result = plugin.DescribeEncounter(encounterName);

        // Assert
        result.ShouldContain(encounterName);
        result.ShouldContain(expectedDescription);
    }

    [Theory]
    [InlineData("GOBLIN AMBUSH")]
    [InlineData("goblin ambush")]
    [InlineData("GoBLiN AmBuSh")]
    public void DescribeEncounter_WithDifferentCasing_FindsEncounter(string encounterName)
    {
        // Arrange
        var plugin = new EncountersPlugin(_adventureWithEncounters);

        // Act
        var result = plugin.DescribeEncounter(encounterName);

        // Assert
        result.ShouldContain("Goblin Ambush"); // Original casing
        result.ShouldContain("A group of goblins attacks from the bushes.");
    }

    [Theory]
    [InlineData("NonexistentEncounter")]
    [InlineData("Unknown")]
    [InlineData("")]
    public void DescribeEncounter_WithInvalidEncounterName_ReturnsErrorMessageAndList(string encounterName)
    {
        // Arrange
        var plugin = new EncountersPlugin(_adventureWithEncounters);

        // Act
        var result = plugin.DescribeEncounter(encounterName);

        // Assert
        result.ShouldContain($"No pre-scripted encounter exists with the name '{encounterName}'.");
        result.ShouldContain("The following encounters have been pre-scripted for the adventure:");
        result.ShouldContain("- Goblin Ambush");
        result.ShouldContain("- Dragon Lair");
        result.ShouldContain("- Riddle Challenge");
        result.ShouldContain("Other encounters can exist, but may not have been pre-scripted");
    }

    [Fact]
    public void DescribeEncounter_WithNoEncountersInAdventure_ReturnsErrorMessage()
    {
        // Arrange
        var plugin = new EncountersPlugin(_adventureWithoutEncounters);

        // Act
        var result = plugin.DescribeEncounter("AnyEncounter");

        // Assert
        result.ShouldContain("No pre-scripted encounter exists with the name 'AnyEncounter'.");
        result.ShouldContain("Other encounters can exist, but may not have been pre-scripted");
    }

    [Fact]
    public void Constructor_WithValidAdventure_InitializesSuccessfully()
    {
        // Arrange & Act
        var plugin = new EncountersPlugin(_adventureWithEncounters);

        // Assert
        plugin.ShouldNotBeNull();
    }

    [Fact]
    public void ListAllEncounters_WithEmptyEncountersOverview_HandlesGracefully()
    {
        // Arrange
        var adventure = new Adventure
        {
            Name = "Test",
            Author = "Test",
            Version = "1.0",
            Ruleset = "Test",
            Backstory = "Test",
            SettingDescription = "Test",
            LocationsOverview = "Test",
            EncountersOverview = "",
            GameMasterNotes = "Test",
            NarrativeStructure = "Test",
            InitialGreetingPrompt = "Test"
        };
        var plugin = new EncountersPlugin(adventure);

        // Act
        var result = plugin.ListAllEncounters();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotContain("You can use the DescribeEncounter command");
    }
}