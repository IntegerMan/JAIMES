using System.Collections.Frozen;
using System.Reflection;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class KernelExtensions
{
    public static IEnumerable<Type> FindPluginTypesWithKernelFunctions()
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract &&
                    type.Name.EndsWith("Plugin") &&
                    type.GetMethods(flags)
                        .Any(m => m.GetCustomAttributes(typeof(KernelFunctionAttribute), inherit: true).Length != 0))
                {
                    yield return type;
                }
            }
        }
    }

    public static IDictionary<string, Type> BuildPluginTypeDictionary() 
        => FindPluginTypesWithKernelFunctions().ToFrozenDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
}