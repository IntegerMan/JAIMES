using MattEland.Jaimes.Core.Models;

namespace MattEland.Jaimes.Core;

public interface ISettingsRoot
{
    AzureOpenAIModelSettings AzureOpenAI { get;}
}