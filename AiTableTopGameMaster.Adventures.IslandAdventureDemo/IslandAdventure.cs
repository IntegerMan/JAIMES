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
        new(ChatRole.Tool, $"Here's the player's character sheet: {CharacterSheet}"),
        new(ChatRole.Tool, $"Here is the game master guidance: {GameMasterNotes}"),
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
                Id = "Wreck Site",
                Description = Resources.LocationWreckSite
            },
            new Location()
            {
                Id = "Collapsed Watchtower",
                Description = Resources.LocationCollapsedWatchtower
            },
            new Location()
            {
                Id = "Ritual Cave",
                Description = Resources.LocationRitualCave
            },
            new Location()
            {
                Id = "Jungle Shack",
                Description = Resources.LocationJungleShack
            },
            new Location()
            {
                Id = "Reef Circle",
                Description = Resources.LocationReefCircle
            }
        ];

        // Encounter data
        EncountersOverview = Resources.Encounters;
        Encounters =
        [
            new Encounter
            {
                Id = "Crab Swarm",
                Description = Resources.EncounterCrabSwarm
            },
            new Encounter
            {
                Id = "Kobold Patrol",
                Description = Resources.EncounterKoboldPatrol
            },
            new Encounter
            {
                Id = "The Ritual",
                Description = Resources.EncounterTheRitual
            },
            new Encounter
            {
                Id = "Trapped Drawer",
                Description = Resources.EncounterTrappedDrawer
            },
            new Encounter
            {
                Id = "Visions of the Deep",
                Description = Resources.EncounterVisionsOfTheDeep
            }
        ];
    }
}