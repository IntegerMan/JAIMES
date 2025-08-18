using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OllamaSharp.Models;
using Serilog;
using KernelExtensions = MattEland.Jaimes.Core.Helpers.KernelExtensions;

namespace MattEland.Jaimes.Core.Models;

public class PluginFactory(ILogger<PluginFactory> log, IServiceProvider sp)
{
    private readonly IDictionary<string, Type> _pluginLookup = 
        KernelExtensions.BuildPluginTypeDictionary();
    
    public void AddPlugins(IKernelBuilder builder, ModelConfiguration model, string[] pluginIds, string agentName)
    {
        if (pluginIds.Length <= 0) return;
        if (!model.SupportsTools)
        {
            Log.Warning("Model {ModelId} does not support tools, but {Name} has plugins. Plugins will be disabled.", model.ModelId, agentName);
            return;
        }

        foreach (var plugin in pluginIds)
        {
            log.LogDebug("Adding plugin {PluginName} to {Name}", plugin, agentName);
            if (!_pluginLookup.TryGetValue(plugin, out Type? pluginType))
            {
                throw new InvalidOperationException($"Plugin type not found: {plugin} for core {agentName}");
            }

            object pluginInstance = sp.GetRequiredService(pluginType);
            builder.Plugins.AddFromObject(pluginInstance);
        }
    }
}