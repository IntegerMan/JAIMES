using AiTableTopGameMaster.Core;
using AiTableTopGameMaster.Core.Plugins.Adventures;
using AiTableTopGameMaster.Domain;
using Microsoft.SemanticKernel;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Core;

public class TableTopKernelExtensionsTests
{
    [Fact]
    public void AddAdventurePlugins_WithValidAdventure_AddsAllPlugins()
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
            InitialGreetingPrompt = "Test greeting",
            PlayerCharacter = new Character
            {
                Name = "Test Character",
                Specialization = "Test Specialization",
                CharacterSheet = "Test Sheet"
            }
        };

        var kernelBuilder = Kernel.CreateBuilder();

        // Act
        var result = kernelBuilder.AddAdventurePlugins(adventure);

        // Assert
        result.ShouldBe(kernelBuilder); // Should return the same builder for fluent interface
        
        // Build the kernel to verify plugins were added
        var kernel = result.Build();
        var plugins = kernel.Plugins;
        
        plugins.ShouldNotBeEmpty();
        
        // Verify that plugin types were added (we can't easily check exact instances)
        var pluginNames = plugins.Select(p => p.Name).ToList();
        pluginNames.ShouldContain("StoryInfoPlugin");
        pluginNames.ShouldContain("LocationsPlugin");
        pluginNames.ShouldContain("EncountersPlugin");
        pluginNames.ShouldContain("CharacterInfoPlugin");
    }

    [Fact]
    public void AddAdventurePlugins_WithMinimalAdventure_StillAddsAllPlugins()
    {
        // Arrange
        var adventure = new Adventure
        {
            Name = "Minimal Adventure",
            Author = "Test Author",
            Version = "1.0.0",
            Ruleset = "D&D 5E",
            Backstory = "",
            SettingDescription = "",
            LocationsOverview = "",
            EncountersOverview = "",
            GameMasterNotes = "",
            NarrativeStructure = "",
            InitialGreetingPrompt = ""
        };

        var kernelBuilder = Kernel.CreateBuilder();

        // Act
        var result = kernelBuilder.AddAdventurePlugins(adventure);

        // Assert
        result.ShouldBe(kernelBuilder);
        
        var kernel = result.Build();
        var plugins = kernel.Plugins;
        
        plugins.ShouldNotBeEmpty();
        plugins.Count.ShouldBe(4); // Should still have all 4 plugin types
    }

    [Fact]
    public void AddAdventurePlugins_CanBeChainedWithOtherKernelBuilderMethods()
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
            InitialGreetingPrompt = "Test greeting"
        };

        // Act & Assert - Should not throw an exception
        var kernel = Kernel.CreateBuilder()
            .AddAdventurePlugins(adventure)
            .Build();

        kernel.ShouldNotBeNull();
        kernel.Plugins.ShouldNotBeEmpty();
    }
}