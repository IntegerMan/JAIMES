using AiTableTopGameMaster.ConsoleApp;
using AiTableTopGameMaster.Domain;
using Microsoft.Extensions.AI;

namespace AiTableTopGameMaster.Adventures.IslandAdventureDemo;

public class IslandAdventure : Adventure
{
    public override string Name => "The Whispering Reef";
    public override string Author => "Matt Eland";
    public override Version Version => new(0, 0, 0, 1);

    public override ICollection<ChatMessage> GenerateInitialHistory() 
        => [
        new(ChatRole.System, Resources.GameMasterSystemPrompt),
        new(ChatRole.Tool, $"Here is the adventure backstory: {Backstory}"),
        new(ChatRole.Tool, $"Here is the adventure setting: {SettingDescription}"),
        new(ChatRole.Tool, "The player character is Emcee, a level 1 rogue. You can check their starting character sheet via a function call if you need to."),
        new(ChatRole.User, Resources.InitialGreetingPrompt),
    ];

    public IslandAdventure()
    {
        Backstory = Resources.Backstory;
        GameMasterNotes = Resources.GameMasterGuidance;
        NarrativeStructure = Resources.NarrativeStructure;
        CharacterSheet = Resources.CharacterSheetRogue;

        // Location Data
        LocationsOverview = Resources.Locations;
        SettingDescription = Resources.IslandDescription;
        Locations =
        [
            new Location
            {
                Name = "Wreck Site",
                Description = Resources.LocationWreckSite
            },
            new Location()
            {
                Name = "Collapsed Watchtower",
                Description = Resources.LocationCollapsedWatchtower
            },
            new Location()
            {
                Name = "Ritual Cave",
                Description = Resources.LocationRitualCave
            },
            new Location()
            {
                Name = "Jungle Shack",
                Description = Resources.LocationJungleShack
            },
            new Location()
            {
                Name = "Reef Circle",
                Description = Resources.LocationReefCircle
            }
        ];

        // Encounter data
        EncountersOverview = Resources.Encounters;
        Encounters =
        [
            new Encounter
            {
                Name = "Crab Swarm",
                Description = Resources.EncounterCrabSwarm
            },
            new Encounter
            {
                Name = "Kobold Patrol",
                Description = Resources.EncounterKoboldPatrol
            },
            new Encounter
            {
                Name = "The Ritual",
                Description = Resources.EncounterTheRitual
            },
            new Encounter
            {
                Name = "Trapped Drawer",
                Description = Resources.EncounterTrappedDrawer
            },
            new Encounter
            {
                Name = "Visions of the Deep",
                Description = Resources.EncounterVisionsOfTheDeep
            }
        ];
    }
}