using System.ComponentModel;
using AiTableTopGameMaster.Domain;
using JetBrains.Annotations;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core.Plugins;

[Description("Provides information about characters in the adventure")]
public class CharacterInfoPlugin(Adventure adventure)
{
    [KernelFunction, UsedImplicitly]
    [Description("Gets the starting character sheet for the player character")]
    public string GetCharacterSheet() => adventure.PlayerCharacter!.CharacterSheet;
}