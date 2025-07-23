using System.ComponentModel;
using System.Text;
using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core.Plugins.Adventures;

[Description("Provides information about the adventure's pre-scripted encounters.")]
public class EncountersPlugin(Adventure adventure)
{
    [KernelFunction, UsedImplicitly]
    [Description("Provides an overview of the adventure's pre-scripted locations and a list of all location names.")]
    public string ListAllEncounters()
    {
        StringBuilder sb = new(adventure.EncountersOverview);
        sb.AppendLine();

        if (adventure.Encounters.Count > 0)
        {
            AddEncounterListToResponse(sb);

            sb.AppendLine();
            sb.AppendLine($"You can use the {nameof(DescribeEncounter)} command to get more details about a specific encounter.");
        }

        return sb.ToString();
    }

    private void AddEncounterListToResponse(StringBuilder sb)
    {
        sb.AppendLine("The following encounters have been pre-scripted for the adventure:");
        foreach (Encounter encounter in adventure.Encounters)
        {
            sb.AppendLine($"- {encounter.Name}");
        }
    }

    [KernelFunction, UsedImplicitly]
    [Description("Gets a details for a specific encounter by its name")]
    public string DescribeEncounter([Description("The name of the encounter")] string encounterName)
    {
        StringBuilder sb = new();
        
        Encounter? encounter = adventure.Encounters.FirstOrDefault(l => l.Name.Equals(encounterName, StringComparison.OrdinalIgnoreCase));
        if (encounter is null)
        {
            sb.AppendLine($"No pre-scripted encounter exists with the name '{encounterName}'.");
            AddEncounterListToResponse(sb);
            sb.AppendLine();
            sb.AppendLine("Other encounters can exist, but may not have been pre-scripted and you'll need to make up their contents.");
        }
        else
        {
            sb.AppendLine(encounter.Name);
            sb.AppendLine();
            sb.AppendLine(encounter.Description);
        }

        return sb.ToString();
    }
}