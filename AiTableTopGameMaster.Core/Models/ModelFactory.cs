using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core.Models;

public class ModelFactory
{
    private readonly IEnumerable<ModelInfo> _models;

    public ModelFactory(IEnumerable<ModelInfo> models)
    {
        _models = models as ModelInfo[] ?? models.ToArray();
        if (!_models.Any())
        {
            throw new ArgumentException("Models collection cannot be empty.", nameof(models));
        }
    }

    public ModelInfo FindModel(string modelId) =>
        _models.FirstOrDefault(m => m.ModelId.Equals(modelId, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Model with ID '{modelId}' not found.", nameof(modelId));

    public IChatClient CreateChatClient(string modelId)
    {
        ModelInfo model = FindModel(modelId);
        if (model.Type != ModelType.Chat) throw new ArgumentException($"Model with ID '{modelId}' is not a chat model.", nameof(modelId));

        return model.Provider switch
        {
            ModelProvider.Ollama => new OllamaChatClient(model.Endpoint, model.ModelId),
            _ => throw new NotSupportedException($"Model provider '{model.Provider}' is not supported.")
        };
    }

    public void AddChatCompletion(IKernelBuilder builder, string modelId)
    {
        ModelInfo model = FindModel(modelId);
        if (model.Type != ModelType.Chat) throw new ArgumentException($"Model with ID '{modelId}' is not a chat model.", nameof(modelId));

        switch (model.Provider)
        {
            case ModelProvider.Ollama:
                builder.AddOllamaChatCompletion(model.ModelId, new Uri(model.Endpoint));
                break;
            default:
                throw new NotSupportedException($"Model provider '{model.Provider}' is not supported.");
                break;
        }
    }
}