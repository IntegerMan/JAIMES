using AiTableTopGameMaster.ConsoleApp.Clients;
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
    public void ConsoleChatClient_ShouldWorkWithAgent()
    {
        // Arrange
        var mockAgent = new Mock<Agent>();
        var console = new TestConsole();
        var logger = NullLogger<ConsoleChatClient>.Instance;
        
        // Act
        var client = new ConsoleChatClient(mockAgent.Object, console, logger);
        
        // Assert
        Assert.NotNull(client);
    }
    
    [Fact]
    public void ChatCompletionAgent_ShouldBeConfiguredProperly()
    {
        // Arrange
        var adventure = CreateTestAdventure();
        var character = CreateTestCharacter();
        var kernel = CreateTestKernel();
        
        // Act
        var agent = new ChatCompletionAgent
        {
            Name = "GameMaster",
            Description = $"Game Master for {adventure.Name} - {adventure.Ruleset} adventure",
            Instructions = BuildTestSystemInstructions(adventure, character),
            Kernel = kernel
        };
        
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
    
    private static string BuildTestSystemInstructions(Adventure adventure, Character playerCharacter)
    {
        return $"""
            {adventure.GameMasterSystemPrompt}
            
            ADVENTURE CONTEXT:
            - Adventure: {adventure.Name} by {adventure.Author}
            - Ruleset: {adventure.Ruleset}
            - Backstory: {adventure.Backstory}
            - Setting: {adventure.SettingDescription}
            
            PLAYER CHARACTER:
            - Name: {playerCharacter.Name}
            - Class/Specialization: {playerCharacter.Specialization}
            - You can check their character sheet via function calls as needed.
            
            NARRATIVE STRUCTURE:
            {adventure.NarrativeStructure}
            
            GAME MASTER NOTES:
            {adventure.GameMasterNotes}
            
            LOCATIONS OVERVIEW:
            {adventure.LocationsOverview}
            
            ENCOUNTERS OVERVIEW:
            {adventure.EncountersOverview}
            
            Remember: You have access to various functions to look up character information, 
            location details, encounter specifics, and sourcebook references. Use these tools 
            to provide rich, accurate gameplay experiences.
            """;
    }
}