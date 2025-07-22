using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.ConsoleApp.Helpers;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Spectre.Console.Testing;
using Xunit;

namespace AiTableTopGameMaster.Tests;

public class AgentChatClientTests
{
    [Fact]
    public void AgentChatClient_ShouldHaveGameMasterAgent()
    {
        // Arrange
        var mockAgent = new Mock<Agent>();
        var console = new TestConsole();
        var logger = NullLogger<AgentChatClient>.Instance;
        
        // Act
        var client = new AgentChatClient(mockAgent.Object, console, logger);
        
        // Assert
        Assert.NotNull(client.GameMasterAgent);
        Assert.Equal(mockAgent.Object, client.GameMasterAgent);
    }
    
    [Fact]
    public void GameMasterAgentFactory_ShouldCreateValidAgent()
    {
        // Arrange
        var factory = new GameMasterAgentFactory(NullLogger<GameMasterAgentFactory>.Instance);
        var adventure = CreateTestAdventure();
        var character = CreateTestCharacter();
        var kernel = CreateTestKernel();
        
        // Act
        var agent = factory.CreateGameMasterAgent(adventure, character, kernel);
        
        // Assert
        Assert.NotNull(agent);
        Assert.Equal("GameMaster", agent.Name);
        Assert.Contains(adventure.Name, agent.Description);
        Assert.Contains(adventure.GameMasterSystemPrompt, agent.Instructions);
        Assert.Contains(character.Name, agent.Instructions);
        Assert.Equal(kernel, agent.Kernel);
    }
    
    private static Adventure CreateTestAdventure()
    {
        return new Adventure
        {
            Name = "Test Adventure",
            Author = "Test Author",
            Version = "1.0",
            Ruleset = "D&D 5E",
            Backstory = "Test backstory",
            SettingDescription = "Test setting",
            LocationsOverview = "Test locations",
            EncountersOverview = "Test encounters",
            GameMasterNotes = "Test notes",
            NarrativeStructure = "Test structure",
            GameMasterSystemPrompt = "You are a test game master",
            InitialGreetingPrompt = "Welcome to the test adventure"
        };
    }
    
    private static Character CreateTestCharacter()
    {
        return new Character
        {
            Name = "Test Character",
            Specialization = "Test Class",
            CharacterSheet = "Test character sheet data"
        };
    }
    
    private static Kernel CreateTestKernel()
    {
        return Kernel.CreateBuilder().Build();
    }
}