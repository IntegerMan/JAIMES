using System.ClientModel;
using AiTableTopGameMaster.Core.Cores;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Serilog;

namespace AiTableTopGameMaster.Core.Models;

public class ModelFactory : IModelFactory
{
    private readonly ILogger<ModelFactory> _log;
    private readonly IServiceProvider _sp;
    private readonly IEnumerable<ModelInfo> _models;
    private readonly IDictionary<string, Type> _pluginLookup;

    public ModelFactory(IEnumerable<ModelInfo> models, ILogger<ModelFactory> log, IServiceProvider sp)
    {
        _log = log;
        _sp = sp;
        _models = models as ModelInfo[] ?? models.ToArray();
        
        if (!_models.Any())
        {
            throw new ArgumentException("Models collection cannot be empty.", nameof(models));
        }
        
        _pluginLookup = AiTableTopGameMaster.Core.Helpers.KernelExtensions.BuildPluginTypeDictionary();
    }

    private ModelInfo FindModel(string modelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId, nameof(modelId));
        return _models.FirstOrDefault(m => m.Id.Equals(modelId, StringComparison.OrdinalIgnoreCase))
               ?? throw new ArgumentException($"Model with ID '{modelId}' not found.", nameof(modelId));
    }

    public IChatClient CreateChatClient(string modelId)
    {
        ModelInfo model = FindModel(modelId);
        if (model.Type != ModelType.Chat) throw new ArgumentException($"Model with ID '{modelId}' is not a chat model.", nameof(modelId));

        return model.Provider switch
        {
            ModelProvider.Ollama => new OllamaChatClient(model.Endpoint, model.ModelId),
            ModelProvider.AzureOpenAI => 
                new AzureOpenAIClient(new Uri(model.Endpoint), new ApiKeyCredential(_sp.GetRequiredService<AzureOpenAIModelSettings>().Key!))
                    .GetChatClient(model.ModelId)
                    .AsIChatClient(),
            _ => throw new NotSupportedException($"Model provider '{model.Provider}' is not supported.")
        };
    }

    public void ConfigureKernel(IKernelBuilder builder, CoreInfo core)
    {
        string agentName = core.Name;
        ModelInfo model = FindModel(core.ModelId);
        string[] plugins = core.Plugins;
        
        ConfigureKernel(builder, agentName, model, plugins);
    }
    
    public void ConfigureKernel(IKernelBuilder builder, string agentName, string modelId, string[] plugins)
    {
        ModelInfo model = FindModel(modelId);

        ConfigureKernel(builder, agentName, model, plugins);
    }

    public void ConfigureKernel(IKernelBuilder builder, string agentName, ModelInfo model, string[] plugins)
    {
        if (model.Type != ModelType.Chat) throw new ArgumentException($"{model.ModelId} is a {model.Type}, not a {ModelType.Chat} model but is referenced by {agentName} as {model.Id}.", nameof(model));

        switch (model.Provider)
        {
            case ModelProvider.Ollama:
                builder.AddOllamaChatCompletion(model.ModelId, new Uri(model.Endpoint));
                break;
            case ModelProvider.AzureOpenAI:
                AzureOpenAIModelSettings azureSettings = _sp.GetRequiredService<AzureOpenAIModelSettings>();
                string azKey = azureSettings.Key ?? throw new InvalidOperationException("Azure OpenAI key is not configured.");
                builder.AddAzureOpenAIChatCompletion(model.ModelId, model.Endpoint, azKey);
                break;
            default:
                throw new NotSupportedException($"Model provider '{model.Provider}' is not supported.");
        }

        // Add Plugins as requested by the core
        AddPlugins(builder, model, plugins, agentName);
    }

    private void AddPlugins(IKernelBuilder builder, ModelInfo model, string[] pluginIds, string agentName)
    {
        if (pluginIds.Length <= 0) return;
        if (!model.SupportsTools)
        {
            Log.Warning("Model {ModelId} does not support tools, but {Name} has plugins. Plugins will be disabled.", model.ModelId, agentName);
            return;
        }

        foreach (var plugin in pluginIds)
        {
            _log.LogDebug("Adding plugin {PluginName} to {Name}", plugin, agentName);
            if (!_pluginLookup.TryGetValue(plugin, out Type? pluginType))
            {
                throw new InvalidOperationException($"Plugin type not found: {plugin} for core {agentName}");
            }

            object pluginInstance = _sp.GetRequiredService(pluginType);
            builder.Plugins.AddFromObject(pluginInstance);
        }
    }
}