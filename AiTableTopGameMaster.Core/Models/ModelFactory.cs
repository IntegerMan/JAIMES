using Microsoft.Extensions.AI;

namespace AiTableTopGameMaster.Core.Models;

public class ModelFactory(IEnumerable<ModelInfo> models)
{
    public ModelInfo FindModel(string modelId) =>
        models.FirstOrDefault(m => m.ModelId.Equals(modelId, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Model with ID '{modelId}' not found.", nameof(modelId));

    public IChatClient CreateChatClient(string modelId)
    {
        ModelInfo model = FindModel(modelId);
        
        if (model.Type != ModelType.Chat)
        {
            throw new ArgumentException($"Model with ID '{modelId}' is not a chat model.", nameof(modelId));
        }

        return model.Provider switch
        {
            ModelProvider.Ollama => new OllamaChatClient(model.Endpoint, model.ModelId),
            _ => throw new NotSupportedException($"Model provider '{model.Provider}' is not supported.")
        };
    }
}