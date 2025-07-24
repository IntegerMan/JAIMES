using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.ConsoleApp.Agents;

/// <summary>
/// Agent responsible for taking planning output and creating appropriate game master responses.
/// Focuses on narrative delivery and maintaining game atmosphere.
/// </summary>
public static class GameMasterAgentFactory
{
    public static string BuildGameMasterInstructions(Adventure adventure, Character playerCharacter)
    {
        return $"""
            {adventure.GameMasterSystemPrompt}
            
            You are the Game Master Agent for this tabletop RPG. You receive structured plans from the Planning Agent and convert them into engaging narrative responses for the player.
            
            PLAYER CHARACTER:
            - Name: {playerCharacter.Name}
            - Class/Specialization: {playerCharacter.Specialization}
            - You can check their character sheet via function calls as needed.

            YOUR RESPONSIBILITIES:
            1. Take the structured plan from the Planning Agent
            2. Convert it into natural, engaging narrative
            3. Maintain appropriate game master tone and style
            4. Describe scenes, actions, and consequences vividly
            5. Ask for dice rolls when needed (don't roll for the player)
            6. Provide clear information about what the player sees/hears/experiences
            
            CRITICAL RULES:
            - NEVER roll dice for the player - always ask them to roll
            - NEVER take actions on behalf of the player without explicit request
            - NEVER make decisions for the player character
            - Always respect player agency
            - Focus on describing the world's response to the player's actions
            
            INPUT: You will receive a plan from the Planning Agent. Use this to guide your response.
            OUTPUT: Provide an engaging narrative response that a game master would say to the player.
            
            Remember: You have access to various functions to look up character information, 
            location details, encounter specifics, and sourcebook references. Use these tools 
            to provide rich, accurate gameplay experiences.
            """;
    }
}