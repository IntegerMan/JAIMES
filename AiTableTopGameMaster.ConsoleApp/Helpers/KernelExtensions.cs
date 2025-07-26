using System.Reflection;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.ConsoleApp.Helpers;

public static class KernelExtensions
{
    public static IEnumerable<Type> FindPluginTypesWithKernelFunctions()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAbstract &&
                    type.Name.EndsWith("Plugin") &&
                    type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Any(m => m.GetCustomAttributes(typeof(KernelFunctionAttribute), inherit: true).Length != 0))
                {
                    yield return type;
                }
            }
        }
    }
}