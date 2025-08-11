using System.ComponentModel;
using JetBrains.Annotations;
using MattEland.Jaimes.Core.Domain;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Core.Plugins.Adventures;

[Description("Provides information about the adventure's overall story and setting.")]
public class StoryInfoPlugin(Adventure adventure)
{
    [KernelFunction, UsedImplicitly]
    [Description("Describes the setting of the adventure")]
    public string DescribeAdventureSetting() => adventure.SettingDescription;

    [KernelFunction, UsedImplicitly]
    [Description("Retrieves the backstory of the adventure")]
    public string GetBackstory() => adventure.Backstory;
    
    [KernelFunction, UsedImplicitly]
    [Description("Retrieves notes and suggestions for the Game Master to help them run the adventure smoothly.")]
    public string GetGameMasterNotes() => adventure.GameMasterNotes;    
    
    [KernelFunction, UsedImplicitly]
    [Description("Retrieves the overall narrative structure intended for the game session.")]
    public string GetNarrativeStructure() => adventure.NarrativeStructure;
}