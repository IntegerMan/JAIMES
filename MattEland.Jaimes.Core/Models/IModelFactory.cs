using AiTableTopGameMaster.Core.Cores;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;

namespace AiTableTopGameMaster.Core.Models;

public interface IModelFactory
{
    IChatClient CreateChatClient(string modelId);
    void ConfigureKernel(IKernelBuilder builder, CoreInfo core);
    void ConfigureKernel(IKernelBuilder builder, string agentName, string modelId, string[] plugins);
    void ConfigureKernel(IKernelBuilder builder, string agentName, ModelInfo model, string[] plugins);
    ModelInfo FindModel(string modelId);
}