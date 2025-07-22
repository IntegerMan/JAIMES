using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

/// <summary>
/// Factory service for creating and configuring game master agents.
/// Handles the conversion from Adventure domain model to agent configuration.
/// </summary>
[UsedImplicitly]
public class GameMasterAgentFactory(ILogger<GameMasterAgentFactory> log)
{
    /// <summary>
    /// Creates a ChatCompletionAgent configured as a Game Master for the specified adventure.
    /// </summary>
    /// <param name="adventure">The adventure to create a game master for</param>
    /// <param name="playerCharacter">The player character for this session</param>
    /// <param name="kernel">The semantic kernel instance with configured services</param>
    /// <returns>Configured ChatCompletionAgent</returns>
    public ChatCompletionAgent CreateGameMasterAgent(Adventure adventure, Character playerCharacter, Kernel kernel)
    {
        log.LogDebug("Creating GameMaster agent for adventure: {AdventureName}", adventure.Name);
        
        // Build the system instructions combining adventure context
        string systemInstructions = BuildSystemInstructions(adventure, playerCharacter);
        
        // EXTENSION POINT: Agent configuration could be enhanced for multi-agent scenarios
        // For example, different agent types could have different configurations:
        // - GameMasterAgent: Narrative control, rule enforcement
        // - NPCAgent: Character roleplay for specific NPCs
        // - WorldAgent: Environmental descriptions and world state
        
        var agent = new ChatCompletionAgent
        {
            Name = "GameMaster",
            Description = $"Game Master for {adventure.Name} - {adventure.Ruleset} adventure",
            Instructions = systemInstructions,
            Kernel = kernel
        };
        
        log.LogInformation("Created GameMaster agent for {Adventure} with {CharacterName}", 
            adventure.Name, playerCharacter.Name);
            
        return agent;
    }
    
    /// <summary>
    /// Builds comprehensive system instructions for the Game Master agent.
    /// Combines adventure context, character information, and game rules.
    /// </summary>
    private static string BuildSystemInstructions(Adventure adventure, Character playerCharacter)
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