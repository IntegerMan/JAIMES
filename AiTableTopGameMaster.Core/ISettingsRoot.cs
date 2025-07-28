using AiTableTopGameMaster.Core.Models;

namespace AiTableTopGameMaster.Core;

public interface ISettingsRoot
{
    AzureOpenAIModelSettings AzureOpenAI { get;}
}