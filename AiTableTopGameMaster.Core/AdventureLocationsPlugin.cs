using System.ComponentModel;
using System.Text;
using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core;

[UsedImplicitly]
public class AdventureLocationsPlugin(Adventure adventure)
{
    [KernelFunction, UsedImplicitly]
    [Description("Describes the setting of the adventure")]
    public string DescribeAdventureSetting() => adventure.SettingDescription;

    [KernelFunction, UsedImplicitly]
    [Description("Provides an overview of the adventure's pre-scripted locations and a list of all location names.")]
    public string DescribeAdventureLocations()
    {
        StringBuilder sb = new(adventure.LocationsOverview);
        sb.AppendLine();

        if (adventure.Locations.Count > 0)
        {
            AddLocationListToResponse(sb);

            sb.AppendLine();
            sb.AppendLine($"You can use the {nameof(DescribeLocation)} command to get more details about a specific location.");
        }

        return sb.ToString();
    }

    private void AddLocationListToResponse(StringBuilder sb)
    {
        sb.AppendLine("The following locations have been pre-scripted for the adventure:");
        foreach (var location in adventure.Locations)
        {
            sb.AppendLine($"- {location.Name}");
        }
    }

    [KernelFunction, UsedImplicitly]
    [Description("Gets a description of the adventure locations.")]
    public string DescribeLocation([Description("The name of the location")] string locationName)
    {
        StringBuilder sb = new();
        
        Location? location = adventure.Locations.FirstOrDefault(l => l.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase));
        if (location is null)
        {
            sb.AppendLine($"No pre-scripted location exists with the name '{locationName}'.");
            AddLocationListToResponse(sb);
            sb.AppendLine();
            sb.AppendLine("Other locations can exist, but may not have been pre-scripted and you'll need to make up their contents.");
        }
        else
        {
            sb.AppendLine(location.Name);
            sb.AppendLine();
            sb.AppendLine(location.Description);
        }

        return sb.ToString();
    }
}