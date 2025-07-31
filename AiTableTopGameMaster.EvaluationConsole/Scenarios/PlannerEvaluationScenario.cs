using Microsoft.Extensions.DependencyInjection;

namespace AiTableTopGameMaster.EvaluationConsole.Scenarios;

public class PlannerEvaluationScenario(ServiceProvider services)
    : AdventureEvaluationScenario(services, "Planner")
{
    public override string Name => "GameStart_Planner";

    protected override string CompletenessGroundTruth
        => """ 
           The response should be a short plan intended to aid the game master in starting a tabletop role-playing game session.
           The response should include CHECKS, KEY POINTS, and CAUTIONS sections with minimal formatting.
           The CHECKS section should indicate that no skill checks or dice rolls are needed at this time
           The KEY POINTS section should include the following:
           - The player character's name is Emcee, a level 1 rogue.
           - Emcee is a smuggler or other form of criminal who has been shipwreck
           - They have washed up on a mysterious island after a shipwreck.
           - Mention the coral on the beach
           - Mention the jungle deeper into the island.
           The CAUTIONS section should provide good reminders for the game master
           The response should be brief and focused on the immediate situation, not the adventure.
           The response should be written as a plan, not a narrative.
           """;

    protected override string EquivalenceGroundTruth
        => """
           CHECKS: Not needed.

           KEY POINTS: The player is Emcee, a level 1 rogue. They were on a smuggling ship called the Silver Minnow and have washed up on a mysterious island after a shipwreck.
           Describe the beach, the mysterious coral, and mention the jungle deeper into the island.

           CAUTIONS: Avoid giving away too much information about the island's mysteries. 
           Keep your response brief and focused on the immediate situation. 
           Ask the player what they want to do, but do not provide a list of options or actions.
           """;
}