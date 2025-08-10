using Microsoft.SemanticKernel.ChatCompletion;

namespace MattEland.Jaimes.RAG;

public interface ITranscriptService
{
    public ChatHistory GetChatHistory();
}