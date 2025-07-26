using Microsoft.SemanticKernel.ChatCompletion;

namespace AiTableTopGameMaster.Domain;

public class Adventure
{
    public required string Name { get; init; }
    public required string Author { get; init; }
    public required string Version { get; init; }
    public required string Ruleset { get; set; }
    public required string Backstory { get; init; }

    public required string SettingDescription { get; init; }

    
    public required string LocationsOverview { get; init; }
    public List<Location> Locations { get; init;  } = [];
    
    public required string EncountersOverview { get; init; }
    public List<Encounter> Encounters { get; init; } = [];
    
    public required string GameMasterNotes { get; init; }
    public required string NarrativeStructure { get; init; }
    public required string InitialGreetingPrompt { get; init; }
    
    public List<Character> Characters { get; init; } = [];
    
    public ChatHistory StartGame(Character playerCharacter)
    {
        PlayerCharacter = playerCharacter;
        
        ChatHistory history = new();
        history.AddSystemMessage($"Here is the adventure backstory: {Backstory}");
        history.AddSystemMessage($"Here is the adventure setting: {SettingDescription}");
        history.AddSystemMessage($"The player character is {playerCharacter.Name}, a {playerCharacter.Specialization}. You can check their starting character sheet via a function call if you need to.");
        history.AddUserMessage(InitialGreetingPrompt);
        
        return history;
    }

    public Character? PlayerCharacter { get; set; }
}