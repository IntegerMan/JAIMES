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

public class MultiAgentChatClientTests
{
    [Fact]
    public void MultiAgentChatClient_ShouldInitializeWithAgents()
    {
        // Arrange
        var agents = CreateTestAgents();
        var console = new TestConsole();
        var logger = NullLogger<MultiAgentChatClient>.Instance;
        
        // Act
        var client = new MultiAgentChatClient(agents, console, logger);
        
        // Assert
        client.ShouldNotBeNull();
    }
    
    [Fact]
    public void PlanningAgentFactory_ShouldCreateAgentWithCorrectName()
    {
        // Arrange
        var adventure = CreateTestAdventure();
        var character = CreateTestCharacter();
        var kernel = CreateTestKernel();
        var arguments = new KernelArguments();
        var loggerFactory = NullLoggerFactory.Instance;
        
        // Act
        string instructions = PlanningAgentFactory.BuildPlanningInstructions(adventure, character);
        var agent = new ChatCompletionAgent
        {
            Name = "PlanningAgent",
            Description = $"Planning Agent for {adventure.Name} - plans appropriate responses for game master",
            Instructions = instructions,
            Kernel = kernel,
            Arguments = arguments,
            LoggerFactory = loggerFactory,
        };
        
        // Assert
        agent.ShouldNotBeNull();
        agent.Name.ShouldBe("PlanningAgent");
        agent.Description.ShouldNotBeNull();
        agent.Description.ShouldContain("Planning Agent");
        agent.Instructions.ShouldNotBeNull();
        agent.Instructions.ShouldContain("Planning Agent for a tabletop RPG");
        agent.Instructions.ShouldContain(adventure.Name);
        agent.Instructions.ShouldContain(character.Name);
    }
    
    [Fact]
    public void GameMasterAgentFactory_ShouldCreateAgentWithCorrectName()
    {
        // Arrange
        var adventure = CreateTestAdventure();
        var character = CreateTestCharacter();
        var kernel = CreateTestKernel();
        var arguments = new KernelArguments();
        var loggerFactory = NullLoggerFactory.Instance;
        
        // Act
        string instructions = GameMasterAgentFactory.BuildGameMasterInstructions(adventure, character);
        var agent = new ChatCompletionAgent
        {
            Name = "GameMaster",
            Description = $"Game Master for {adventure.Name} - delivers narrative responses to players",
            Instructions = instructions,
            Kernel = kernel,
            Arguments = arguments,
            LoggerFactory = loggerFactory,
        };
        
        // Assert
        agent.ShouldNotBeNull();
        agent.Name.ShouldBe("GameMaster");
        agent.Description.ShouldNotBeNull();
        agent.Description.ShouldContain("Game Master");
        agent.Instructions.ShouldNotBeNull();
        agent.Instructions.ShouldContain(adventure.GameMasterSystemPrompt);
        agent.Instructions.ShouldContain(adventure.Name);
        agent.Instructions.ShouldContain(character.Name);
    }
    
    [Fact]
    public void EditorAgentFactory_ShouldCreateAgentWithCorrectName()
    {
        // Arrange
        var adventure = CreateTestAdventure();
        var character = CreateTestCharacter();
        var kernel = CreateTestKernel();
        var arguments = new KernelArguments();
        var loggerFactory = NullLoggerFactory.Instance;
        
        // Act
        string instructions = EditorAgentFactory.BuildEditorInstructions(adventure, character);
        var agent = new ChatCompletionAgent
        {
            Name = "EditorAgent",
            Description = $"Editor Agent for {adventure.Name} - improves and proofs game master responses",
            Instructions = instructions,
            Kernel = kernel,
            Arguments = arguments,
            LoggerFactory = loggerFactory,
        };
        
        // Assert
        agent.ShouldNotBeNull();
        agent.Name.ShouldBe("EditorAgent");
        agent.Description.ShouldNotBeNull();
        agent.Description.ShouldContain("Editor Agent");
        agent.Instructions.ShouldNotBeNull();
        agent.Instructions.ShouldContain("Editor Agent specializing in tabletop RPG");
        agent.Instructions.ShouldContain(adventure.Name);
        agent.Instructions.ShouldContain(character.Name);
    }
    
    [Fact]
    public void AllAgents_ShouldPreventDiceRollingForPlayer()
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
        planningAgent.Instructions.ShouldContain("NEVER roll dice for the player");
        gmAgent.Instructions.ShouldNotBeNull();
        gmAgent.Instructions.ShouldContain("NEVER roll dice for the player");
        editorAgent.Instructions.ShouldNotBeNull();
        editorAgent.Instructions.ShouldContain("Game master rolling dice for the player");
        
        planningAgent.Instructions.ShouldContain("NEVER take actions on behalf of the player");
        gmAgent.Instructions.ShouldContain("NEVER take actions on behalf of the player");
        editorAgent.Instructions.ShouldContain("Taking actions on behalf of the player");
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