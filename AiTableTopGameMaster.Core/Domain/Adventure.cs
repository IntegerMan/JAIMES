namespace AiTableTopGameMaster.Core.Domain;

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

    public Character? PlayerCharacter { get; set; }
}