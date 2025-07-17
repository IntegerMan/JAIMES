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
        new(ChatRole.System,
 $"You are an AI game master for a tabletop role-playing game of Dungeons and Dragons 5th Edition. You will interact with the player, who is a human, and provide responses to their queries and actions. You are working through a short demonstration adventure called {Name} by {Author}, but have liberty to improvise and create new content as needed. Keep things fair and challenging and drive the story forward. Let the player tell you what they want, then interpret their response. Do not suggestion actions to the player or take actions on their behalf unless they are blatantly obvious."),
        new(ChatRole.Tool, $"Here is the adventure backstory: {Backstory}"),
        new(ChatRole.User, $"Hello, here's my character sheet at the start of our adventure: \r\n {CharacterSheet}"),
        new(ChatRole.Tool, $"Here is the game master guidance: {GameMasterNotes}"),
        new(ChatRole.User, "Please start the adventure."),
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