using AiTableTopGameMaster.Domain;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core;

public static class TableTopKernelExtensions
{
    public static IKernelBuilder AddAdventurePlugins(this IKernelBuilder builder, Adventure adventure)
    {
        builder.Plugins.AddFromObject(new AdventureLocationsPlugin(adventure));
        
        return builder;
    }
}