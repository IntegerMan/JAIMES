using AiTableTopGameMaster.Core.Domain;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Domain;

public class EncounterTests
{
    [Theory]
    [InlineData("Goblin Ambush", "A group of goblins attacks from the bushes")]
    [InlineData("Dragon Lair", "An ancient red dragon guards its treasure")]
    [InlineData("Riddle of the Sphinx", "A sphinx poses three riddles")]
    public void Encounter_Initialization_SetsPropertiesCorrectly(string name, string description)
    {
        // Arrange & Act
        var encounter = new Encounter
        {
            Name = name,
            Description = description
        };

        // Assert
        encounter.Name.ShouldBe(name);
        encounter.Description.ShouldBe(description);
    }

    [Fact]
    public void Encounter_AsRecord_SupportsValueEquality()
    {
        // Arrange
        var encounter1 = new Encounter
        {
            Name = "Test Encounter",
            Description = "Test Description"
        };
        
        var encounter2 = new Encounter
        {
            Name = "Test Encounter",
            Description = "Test Description"
        };
        
        var encounter3 = new Encounter
        {
            Name = "Different Encounter",
            Description = "Test Description"
        };

        // Act & Assert
        encounter1.ShouldBe(encounter2); // Records with same values are equal
        encounter1.ShouldNotBe(encounter3); // Records with different values are not equal
        encounter1.GetHashCode().ShouldBe(encounter2.GetHashCode()); // Hash codes match for equal records
    }

    [Fact]
    public void Encounter_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var encounter = new Encounter
        {
            Name = "Test Encounter",
            Description = "Test Description"
        };

        // Act
        var result = encounter.ToString();

        // Assert
        result.ShouldContain("Test Encounter");
        result.ShouldContain("Test Description");
    }

    [Theory]
    [InlineData("", "Valid Description")]
    [InlineData("Valid Name", "")]
    [InlineData("", "")]
    public void Encounter_WithEmptyStrings_StillInitializes(string name, string description)
    {
        // Arrange & Act
        var encounter = new Encounter
        {
            Name = name,
            Description = description
        };

        // Assert
        encounter.Name.ShouldBe(name);
        encounter.Description.ShouldBe(description);
    }
}