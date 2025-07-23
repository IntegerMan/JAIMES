using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Shouldly;
using Spectre.Console.Testing;
using Xunit;

namespace AiTableTopGameMaster.Tests;

public class AgentChatClientTests
{
    [Fact]
    public void ConsoleChatClient_ShouldWorkWithAgent()
    {
        // Arrange
        Mock<Agent> mockAgent = new Mock<Agent>();
        TestConsole console = new();
        ILogger<ConsoleChatClient> logger = NullLogger<ConsoleChatClient>.Instance;
        
        // Act
        ConsoleChatClient client = new(mockAgent.Object, console, logger);
        
        // Assert
        client.ShouldNotBeNull();
    }
    
    [Fact]
    public void ChatCompletionAgent_ShouldBeConfiguredProperly()
    {
        // Arrange
        Adventure adventure = CreateTestAdventure();
        Character character = CreateTestCharacter();
        Kernel kernel = CreateTestKernel();
        
        // Act
        ChatCompletionAgent agent = new()
        {
            Name = "GameMaster",
            Description = $"Game Master for {adventure.Name} - {adventure.Ruleset} adventure",
            Instructions = BuildTestSystemInstructions(adventure, character),
            Kernel = kernel
        };
        
        // Assert
        agent.ShouldNotBeNull();
        agent.Name.ShouldBe("GameMaster");
        agent.Description.ShouldContain(adventure.Name);
        agent.Instructions.ShouldContain(adventure.GameMasterSystemPrompt);
        agent.Instructions.ShouldContain(character.Name);
        agent.Kernel.ShouldBe(kernel);
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