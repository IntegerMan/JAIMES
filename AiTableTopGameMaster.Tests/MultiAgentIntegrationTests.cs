using AiTableTopGameMaster.ConsoleApp.Agents;
using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Shouldly;
using Spectre.Console.Testing;

namespace AiTableTopGameMaster.Tests;

public class MultiAgentIntegrationTests
{
    [Fact]
    public void MultiAgentChatClient_ShouldHandleAgentFlow()
    {
        // Arrange
        var agents = CreateTestAgents();
        var console = new TestConsole();
        var logger = NullLogger<MultiAgentChatClient>.Instance;
        var client = new MultiAgentChatClient(agents, console, logger);
        
        // Act & Assert - Just verify the client can be created with the agents
        client.ShouldNotBeNull();
        
        // Verify console shows mode selection
        var adventure = CreateTestAdventure();
        var character = CreateTestCharacter();
        
        // The agents should be properly constructed
        var planningAgent = agents.First(a => a.Name == "PlanningAgent");
        var gmAgent = agents.First(a => a.Name == "GameMaster");
        var editorAgent = agents.First(a => a.Name == "EditorAgent");
        
        planningAgent.ShouldNotBeNull();
        gmAgent.ShouldNotBeNull();
        editorAgent.ShouldNotBeNull();
    }
    
    [Fact]
    public void AllAgents_ShouldHaveUniqueNames()
    {
        // Arrange
        var agents = CreateTestAgents();
        
        // Act
        var names = agents.Select(a => a.Name).ToArray();
        
        // Assert
        names.ShouldBe(new[] { "PlanningAgent", "GameMaster", "EditorAgent" });
        names.Distinct().Count().ShouldBe(3);
    }
    
    [Fact]
    public void Agents_ShouldBeConfiguredWithAdventureContext()
    {
        // Arrange
        var adventure = CreateTestAdventure();
        var character = CreateTestCharacter();
        var kernel = CreateTestKernel();
        var arguments = new KernelArguments();
        var loggerFactory = NullLoggerFactory.Instance;
        
        // Act
        string instructions2 = PlanningAgentFactory.BuildPlanningInstructions(adventure, character);
        var planningAgent = new ChatCompletionAgent
        {
            Name = "PlanningAgent",
            Description = $"Planning Agent for {adventure.Name} - plans appropriate responses for game master",
            Instructions = instructions2,
            Kernel = kernel,
            Arguments = arguments,
            LoggerFactory = loggerFactory,
        };
        string instructions = GameMasterAgentFactory.BuildGameMasterInstructions(adventure, character);
        var gmAgent = new ChatCompletionAgent
        {
            Name = "GameMaster",
            Description = $"Game Master for {adventure.Name} - delivers narrative responses to players",
            Instructions = instructions,
            Kernel = kernel,
            Arguments = arguments,
            LoggerFactory = loggerFactory,
        };
        string instructions1 = EditorAgentFactory.BuildEditorInstructions(adventure, character);
        var editorAgent = new ChatCompletionAgent
        {
            Name = "EditorAgent",
            Description = $"Editor Agent for {adventure.Name} - improves and proofs game master responses",
            Instructions = instructions1,
            Kernel = kernel,
            Arguments = arguments,
            LoggerFactory = loggerFactory,
        };
        
        // Assert
        planningAgent.Instructions.ShouldNotBeNull();
        planningAgent.Instructions.ShouldContain(adventure.Name);
        planningAgent.Instructions.ShouldContain(character.Name);
        
        gmAgent.Instructions.ShouldNotBeNull();
        gmAgent.Instructions.ShouldContain(adventure.GameMasterSystemPrompt);
        gmAgent.Instructions.ShouldContain(adventure.Name);
        gmAgent.Instructions.ShouldContain(character.Name);
        
        editorAgent.Instructions.ShouldNotBeNull();
        editorAgent.Instructions.ShouldContain(adventure.Name);
        editorAgent.Instructions.ShouldContain(character.Name);
    }
    
    private static Agent[] CreateTestAgents()
    {
        var adventure = CreateTestAdventure();
        var character = CreateTestCharacter();
        var kernel = CreateTestKernel();
        var arguments = new KernelArguments();
        var loggerFactory = NullLoggerFactory.Instance;

        string instructions = GameMasterAgentFactory.BuildGameMasterInstructions(adventure, character);
        string instructions1 = EditorAgentFactory.BuildEditorInstructions(adventure, character);
        string instructions2 = PlanningAgentFactory.BuildPlanningInstructions(adventure, character);
        return
        [
            new ChatCompletionAgent
            {
                Name = "PlanningAgent",
                Description = $"Planning Agent for {adventure.Name} - plans appropriate responses for game master",
                Instructions = instructions2,
                Kernel = kernel,
                Arguments = arguments,
                LoggerFactory = loggerFactory,
            },
            new ChatCompletionAgent
            {
                Name = "GameMaster",
                Description = $"Game Master for {adventure.Name} - delivers narrative responses to players",
                Instructions = instructions,
                Kernel = kernel,
                Arguments = arguments,
                LoggerFactory = loggerFactory,
            },
            new ChatCompletionAgent
            {
                Name = "EditorAgent",
                Description = $"Editor Agent for {adventure.Name} - improves and proofs game master responses",
                Instructions = instructions1,
                Kernel = kernel,
                Arguments = arguments,
                LoggerFactory = loggerFactory,
            }
        ];
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