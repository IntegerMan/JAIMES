using Microsoft.Extensions.AI;

namespace AiTableTopGameMaster.Domain;

public class Adventure
{
    public required string Name { get; init; }
    public required string Author { get; init; }
    public required string Version { get; init; }
    public required string Backstory { get; init; }

    public required string SettingDescription { get; init; }

    
    public required string LocationsOverview { get; init; }
    public List<Location> Locations { get; init;  } = [];
    
    public required string EncountersOverview { get; init; }
    public List<Encounter> Encounters { get; init; } = [];
    
    public required string GameMasterNotes { get; init; }
    public required string NarrativeStructure { get; init; }
    public required string CharacterSheet { get; init; }
    public required string GameMasterSystemPrompt { get; init; }
    public required string InitialGreetingPrompt { get; init; }
    
    public List<Character> Characters { get; init; } = [];
    
    public ICollection<ChatMessage> GenerateInitialHistory() 
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, GameMasterSystemPrompt),
            new(ChatRole.Tool, $"Here is the adventure backstory: {Backstory}"),
            new(ChatRole.Tool, $"Here is the adventure setting: {SettingDescription}")
        };

        var playerCharacter = Characters.FirstOrDefault(c => c.IsPlayerCharacter);
        if (playerCharacter != null)
        {
            messages.Add(new(ChatRole.Tool, $"The player character is {playerCharacter.Name}, a {playerCharacter.Level} {playerCharacter.Class}. You can check their starting character sheet via a function call if you need to."));
        }

        messages.Add(new(ChatRole.User, InitialGreetingPrompt));
        
        return messages;
    }
}