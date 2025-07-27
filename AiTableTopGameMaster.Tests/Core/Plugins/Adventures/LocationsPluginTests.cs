using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Plugins.Adventures;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Core.Plugins.Adventures;

public class LocationsPluginTests
{
    private readonly Adventure _adventureWithLocations;
    private readonly Adventure _adventureWithoutLocations;

    public LocationsPluginTests()
    {
        _adventureWithLocations = new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "This adventure features several key locations:",
            EncountersOverview = "Test encounters",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            InitialGreetingPrompt = "Test greeting",
            Locations = 
            [
                new Location { Name = "Tavern", Description = "A cozy inn with warm fires and friendly locals." },
                new Location { Name = "Forest", Description = "A dark woodland filled with mysterious creatures." },
                new Location { Name = "Castle", Description = "An ancient fortress overlooking the valley." }
            ]
        };

        _adventureWithoutLocations = new Adventure
        {
            Name = "Simple Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "This is a simple adventure with no pre-scripted locations.",
            EncountersOverview = "Test encounters",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            InitialGreetingPrompt = "Test greeting"
        };
    }

    [Fact]
    public void ListAllLocations_WithLocations_ReturnsOverviewAndList()
    {
        // Arrange
        var plugin = new LocationsPlugin(_adventureWithLocations);

        // Act
        var result = plugin.ListAllLocations();

        // Assert
        result.ShouldContain("This adventure features several key locations:");
        result.ShouldContain("The following locations have been pre-scripted for the adventure:");
        result.ShouldContain("- Tavern");
        result.ShouldContain("- Forest");
        result.ShouldContain("- Castle");
        result.ShouldContain("You can use the DescribeLocation command");
    }

    [Fact]
    public void ListAllLocations_WithoutLocations_ReturnsOnlyOverview()
    {
        // Arrange
        var plugin = new LocationsPlugin(_adventureWithoutLocations);

        // Act
        var result = plugin.ListAllLocations();

        // Assert
        result.ShouldContain("This is a simple adventure with no pre-scripted locations.");
        result.ShouldNotContain("The following locations have been pre-scripted for the adventure:");
        result.ShouldNotContain("You can use the DescribeLocation command");
    }

    [Theory]
    [InlineData("Tavern", "A cozy inn with warm fires and friendly locals.")]
    [InlineData("Forest", "A dark woodland filled with mysterious creatures.")]
    [InlineData("Castle", "An ancient fortress overlooking the valley.")]
    public void DescribeLocation_WithValidLocationName_ReturnsLocationDetails(string locationName, string expectedDescription)
    {
        // Arrange
        var plugin = new LocationsPlugin(_adventureWithLocations);

        // Act
        var result = plugin.DescribeLocation(locationName);

        // Assert
        result.ShouldContain(locationName);
        result.ShouldContain(expectedDescription);
    }

    [Theory]
    [InlineData("TAVERN")]
    [InlineData("tavern")]
    [InlineData("TaVeRn")]
    public void DescribeLocation_WithDifferentCasing_FindsLocation(string locationName)
    {
        // Arrange
        var plugin = new LocationsPlugin(_adventureWithLocations);

        // Act
        var result = plugin.DescribeLocation(locationName);

        // Assert
        result.ShouldContain("Tavern"); // Original casing
        result.ShouldContain("A cozy inn with warm fires and friendly locals.");
    }

    [Theory]
    [InlineData("NonexistentLocation")]
    [InlineData("Unknown")]
    [InlineData("")]
    public void DescribeLocation_WithInvalidLocationName_ReturnsErrorMessageAndList(string locationName)
    {
        // Arrange
        var plugin = new LocationsPlugin(_adventureWithLocations);

        // Act
        var result = plugin.DescribeLocation(locationName);

        // Assert
        result.ShouldContain($"No pre-scripted location exists with the name '{locationName}'.");
        result.ShouldContain("The following locations have been pre-scripted for the adventure:");
        result.ShouldContain("- Tavern");
        result.ShouldContain("- Forest");
        result.ShouldContain("- Castle");
        result.ShouldContain("Other locations can exist, but may not have been pre-scripted");
    }

    [Fact]
    public void DescribeLocation_WithNoLocationsInAdventure_ReturnsErrorMessage()
    {
        // Arrange
        var plugin = new LocationsPlugin(_adventureWithoutLocations);

        // Act
        var result = plugin.DescribeLocation("AnyLocation");

        // Assert
        result.ShouldContain("No pre-scripted location exists with the name 'AnyLocation'.");
        result.ShouldContain("Other locations can exist, but may not have been pre-scripted");
    }

    [Fact]
    public void Constructor_WithValidAdventure_InitializesSuccessfully()
    {
        // Arrange & Act
        var plugin = new LocationsPlugin(_adventureWithLocations);

        // Assert
        plugin.ShouldNotBeNull();
    }

    [Fact]
    public void ListAllLocations_WithEmptyLocationsOverview_HandlesGracefully()
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
            LocationsOverview = "",
            EncountersOverview = "Test",
            GameMasterNotes = "Test",
            NarrativeStructure = "Test",
            InitialGreetingPrompt = "Test"
        };
        var plugin = new LocationsPlugin(adventure);

        // Act
        var result = plugin.ListAllLocations();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotContain("You can use the DescribeLocation command");
    }
}