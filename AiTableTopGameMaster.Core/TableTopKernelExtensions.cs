using AiTableTopGameMaster.Domain;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core;

public static class TableTopKernelExtensions
{
    public static Kernel AddAdventurePlugins(this Kernel kernel, Adventure adventure)
    {
        kernel.Plugins.AddFromObject(new AdventureLocationsPlugin(adventure));
        
        return kernel;
    }
}