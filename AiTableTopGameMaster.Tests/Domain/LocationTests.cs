using AiTableTopGameMaster.Core.Domain;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Domain;

public class LocationTests
{
    [Theory]
    [InlineData("Tavern", "A cozy inn with warm fires")]
    [InlineData("Forest", "A dark and mysterious woodland")]
    [InlineData("Castle", "An imposing fortress on a hill")]
    public void Location_Initialization_SetsPropertiesCorrectly(string name, string description)
    {
        // Arrange & Act
        var location = new Location
        {
            Name = name,
            Description = description
        };

        // Assert
        location.Name.ShouldBe(name);
        location.Description.ShouldBe(description);
    }

    [Fact]
    public void Location_AsRecord_SupportsValueEquality()
    {
        // Arrange
        var location1 = new Location
        {
            Name = "Test Location",
            Description = "Test Description"
        };
        
        var location2 = new Location
        {
            Name = "Test Location",
            Description = "Test Description"
        };
        
        var location3 = new Location
        {
            Name = "Different Location",
            Description = "Test Description"
        };

        // Act & Assert
        location1.ShouldBe(location2); // Records with same values are equal
        location1.ShouldNotBe(location3); // Records with different values are not equal
        location1.GetHashCode().ShouldBe(location2.GetHashCode()); // Hash codes match for equal records
    }

    [Fact]
    public void Location_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var location = new Location
        {
            Name = "Test Location",
            Description = "Test Description"
        };

        // Act
        var result = location.ToString();

        // Assert
        result.ShouldContain("Test Location");
        result.ShouldContain("Test Description");
    }

    [Theory]
    [InlineData("", "Valid Description")]
    [InlineData("Valid Name", "")]
    [InlineData("", "")]
    public void Location_WithEmptyStrings_StillInitializes(string name, string description)
    {
        // Arrange & Act
        var location = new Location
        {
            Name = name,
            Description = description
        };

        // Assert
        location.Name.ShouldBe(name);
        location.Description.ShouldBe(description);
    }
}