using AiTableTopGameMaster.Core.Domain;
using AiTableTopGameMaster.Core.Plugins;
using AiTableTopGameMaster.Core.Plugins.Adventures;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core;

public static class TableTopKernelExtensions
{
    public static IKernelBuilder AddAdventurePlugins(this IKernelBuilder builder, Adventure adventure)
    {
        builder.Plugins.AddFromObject(new StoryInfoPlugin(adventure));
        builder.Plugins.AddFromObject(new LocationsPlugin(adventure));
        builder.Plugins.AddFromObject(new EncountersPlugin(adventure));
        builder.Plugins.AddFromObject(new CharacterInfoPlugin(adventure));
        
        return builder;
    }
}