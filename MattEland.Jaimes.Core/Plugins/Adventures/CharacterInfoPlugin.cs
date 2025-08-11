using System.ComponentModel;
using JetBrains.Annotations;
using MattEland.Jaimes.Core.Domain;
using Microsoft.SemanticKernel;

namespace MattEland.Jaimes.Core.Plugins.Adventures;

[Description("Provides information about characters in the adventure")]
public class CharacterInfoPlugin(Adventure adventure)
{
    [KernelFunction, UsedImplicitly]
    [Description("Gets the starting character sheet for the player character")]
    public string GetCharacterSheet() => adventure.PlayerCharacter!.CharacterSheet;
}