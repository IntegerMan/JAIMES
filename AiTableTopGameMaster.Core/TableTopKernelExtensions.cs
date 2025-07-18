using AiTableTopGameMaster.Domain;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core;

public static class TableTopKernelExtensions
{
    public static IKernelBuilder AddAdventurePlugins(this IKernelBuilder builder, Adventure adventure)
    {
        // Register any services or extensions needed for the tabletop game master functionality
        // For example, you might add custom skills, plugins, or services here.
        
        return builder;
    }
}