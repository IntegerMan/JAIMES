using Microsoft.Extensions.AI;

namespace AiTableTopGameMaster.Domain;

public abstract class Adventure
{
    public abstract string Name { get;  }
    public abstract string Author { get;  }
    public abstract Version Version { get;  }
    public required string Backstory { get; init; }

    public required string SettingDescription { get; init; }

    
    public required string LocationsOverview { get; init; }
    public List<Location> Locations { get; init;  } = [];
    
    public required string EncountersOverview { get; init; }
    public List<Encounter> Encounters { get; init; } = [];
    
    public required string GameMasterNotes { get; init; }
    public required string NarrativeStructure { get; init; }
    public required string CharacterSheet { get; init; }
    public abstract ICollection<ChatMessage> GenerateInitialHistory();
}