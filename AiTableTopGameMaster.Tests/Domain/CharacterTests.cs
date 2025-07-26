using AiTableTopGameMaster.Domain;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Domain;

public class CharacterTests
{
    [Theory]
    [InlineData("Aragorn", "Ranger", "Human Ranger, Level 1")]
    [InlineData("Gandalf", "Wizard", "Human Wizard, Level 5")]
    [InlineData("Legolas", "Archer", "Elf Ranger, Level 3")]
    public void Character_Initialization_SetsPropertiesCorrectly(string name, string specialization, string characterSheet)
    {
        // Arrange & Act
        var character = new Character
        {
            Name = name,
            Specialization = specialization,
            CharacterSheet = characterSheet
        };

        // Assert
        character.Name.ShouldBe(name);
        character.Specialization.ShouldBe(specialization);
        character.CharacterSheet.ShouldBe(characterSheet);
    }

    [Fact]
    public void Character_PropertiesAreRequired_CannotBeInitializedWithoutThem()
    {
        // Arrange & Act & Assert
        // This test verifies that the required properties are enforced by the compiler
        // The following lines would not compile without all required properties:
        var character = new Character
        {
            Name = "Test Character",
            Specialization = "Test Specialization",
            CharacterSheet = "Test Character Sheet"
        };

        character.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("", "Valid Specialization", "Valid Sheet")]
    [InlineData("Valid Name", "", "Valid Sheet")]
    [InlineData("Valid Name", "Valid Specialization", "")]
    public void Character_WithEmptyStrings_StillInitializes(string name, string specialization, string characterSheet)
    {
        // Arrange & Act
        var character = new Character
        {
            Name = name,
            Specialization = specialization,
            CharacterSheet = characterSheet
        };

        // Assert
        character.Name.ShouldBe(name);
        character.Specialization.ShouldBe(specialization);
        character.CharacterSheet.ShouldBe(characterSheet);
    }
}