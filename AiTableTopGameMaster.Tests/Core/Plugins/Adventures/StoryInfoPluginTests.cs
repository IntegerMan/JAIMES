using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Plugins.Adventures;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Core.Plugins.Adventures;

public class StoryInfoPluginTests
{
    private readonly Adventure _testAdventure;
    private readonly StoryInfoPlugin _plugin;

    public StoryInfoPluginTests()
    {
        _testAdventure = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "Long ago, in a kingdom far away...",
            SettingDescription = "A medieval fantasy world with magic and dragons",
            LocationsOverview = "Various locations overview",
            EncountersOverview = "Various encounters overview",
            GameMasterNotes = "Important notes for the GM to remember",
            NarrativeStructure = "Three-act structure with rising action",
            InitialGreetingPrompt = "Welcome to the adventure"
        };
        
        _plugin = new StoryInfoPlugin(_testAdventure);
    }

    [Fact]
    public void DescribeAdventureSetting_ReturnsCorrectSetting()
    {
        // Arrange
        // (Setup done in constructor)

        // Act
        var result = _plugin.DescribeAdventureSetting();

        // Assert
        result.ShouldBe("A medieval fantasy world with magic and dragons");
    }

    [Fact]
    public void GetBackstory_ReturnsCorrectBackstory()
    {
        // Arrange
        // (Setup done in constructor)

        // Act
        var result = _plugin.GetBackstory();

        // Assert
        result.ShouldBe("Long ago, in a kingdom far away...");
    }

    [Fact]
    public void GetGameMasterNotes_ReturnsCorrectNotes()
    {
        // Arrange
        // (Setup done in constructor)

        // Act
        var result = _plugin.GetGameMasterNotes();

        // Assert
        result.ShouldBe("Important notes for the GM to remember");
    }

    [Fact]
    public void GetNarrativeStructure_ReturnsCorrectStructure()
    {
        // Arrange
        // (Setup done in constructor)

        // Act
        var result = _plugin.GetNarrativeStructure();

        // Assert
        result.ShouldBe("Three-act structure with rising action");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void DescribeAdventureSetting_WithEmptyOrNullSetting_ReturnsAsIs(string? setting)
    {
        // Arrange
        var adventure = new Adventure
        {
            Name = "Test",
            Author = "Test",
            Version = "1.0",
            Ruleset = "Test",
            Backstory = "Test",
            SettingDescription = setting,
            LocationsOverview = "Test",
            EncountersOverview = "Test",
            GameMasterNotes = "Test",
            NarrativeStructure = "Test",
            InitialGreetingPrompt = "Test"
        };
        var plugin = new StoryInfoPlugin(adventure);

        // Act
        var result = plugin.DescribeAdventureSetting();

        // Assert
        result.ShouldBe(setting);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetBackstory_WithEmptyOrNullBackstory_ReturnsAsIs(string? backstory)
    {
        // Arrange
        var adventure = new Adventure
        {
            Name = "Test",
            Author = "Test",
            Version = "1.0",
            Ruleset = "Test",
            Backstory = backstory,
            SettingDescription = "Test",
            LocationsOverview = "Test",
            EncountersOverview = "Test",
            GameMasterNotes = "Test",
            NarrativeStructure = "Test",
            InitialGreetingPrompt = "Test"
        };
        var plugin = new StoryInfoPlugin(adventure);

        // Act
        var result = plugin.GetBackstory();

        // Assert
        result.ShouldBe(backstory);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetGameMasterNotes_WithEmptyOrNullNotes_ReturnsAsIs(string? notes)
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
            EncountersOverview = "Test",
            GameMasterNotes = notes,
            NarrativeStructure = "Test",
            InitialGreetingPrompt = "Test"
        };
        var plugin = new StoryInfoPlugin(adventure);

        // Act
        var result = plugin.GetGameMasterNotes();

        // Assert
        result.ShouldBe(notes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetNarrativeStructure_WithEmptyOrNullStructure_ReturnsAsIs(string? structure)
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
            EncountersOverview = "Test",
            GameMasterNotes = "Test",
            NarrativeStructure = structure,
            InitialGreetingPrompt = "Test"
        };
        var plugin = new StoryInfoPlugin(adventure);

        // Act
        var result = plugin.GetNarrativeStructure();

        // Assert
        result.ShouldBe(structure);
    }
}