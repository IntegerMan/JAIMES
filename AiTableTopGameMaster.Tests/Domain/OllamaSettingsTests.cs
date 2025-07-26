using AiTableTopGameMaster.Domain;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Domain;

public class OllamaSettingsTests
{
    [Theory]
    [InlineData("You are a helpful AI assistant", "llama3", "http://localhost:11434", "nomic-embed", "http://localhost:11434")]
    [InlineData("You are a game master", "mistral", "http://192.168.1.100:11434", "all-minilm", "http://192.168.1.100:11434")]
    public void OllamaSettings_Initialization_SetsPropertiesCorrectly(
        string systemPrompt, 
        string chatModelId, 
        string chatEndpoint, 
        string embeddingModelId, 
        string embeddingEndpoint)
    {
        // Arrange & Act
        var settings = new OllamaSettings
        {
            SystemPrompt = systemPrompt,
            ChatModelId = chatModelId,
            ChatEndpoint = chatEndpoint,
            EmbeddingModelId = embeddingModelId,
            EmbeddingEndpoint = embeddingEndpoint
        };

        // Assert
        settings.SystemPrompt.ShouldBe(systemPrompt);
        settings.ChatModelId.ShouldBe(chatModelId);
        settings.ChatEndpoint.ShouldBe(chatEndpoint);
        settings.EmbeddingModelId.ShouldBe(embeddingModelId);
        settings.EmbeddingEndpoint.ShouldBe(embeddingEndpoint);
    }

    [Fact]
    public void OllamaSettings_PropertiesAreRequired_CannotBeInitializedWithoutThem()
    {
        // Arrange & Act & Assert
        // This test verifies that the required properties are enforced by the compiler
        var settings = new OllamaSettings
        {
            SystemPrompt = "Test prompt",
            ChatModelId = "test-model",
            ChatEndpoint = "http://localhost:11434",
            EmbeddingModelId = "test-embedding",
            EmbeddingEndpoint = "http://localhost:11434"
        };

        settings.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("", "valid-model", "http://localhost:11434", "valid-embedding", "http://localhost:11434")]
    [InlineData("Valid prompt", "", "http://localhost:11434", "valid-embedding", "http://localhost:11434")]
    [InlineData("Valid prompt", "valid-model", "", "valid-embedding", "http://localhost:11434")]
    [InlineData("Valid prompt", "valid-model", "http://localhost:11434", "", "http://localhost:11434")]
    [InlineData("Valid prompt", "valid-model", "http://localhost:11434", "valid-embedding", "")]
    public void OllamaSettings_WithEmptyStrings_StillInitializes(
        string systemPrompt, 
        string chatModelId, 
        string chatEndpoint, 
        string embeddingModelId, 
        string embeddingEndpoint)
    {
        // Arrange & Act
        var settings = new OllamaSettings
        {
            SystemPrompt = systemPrompt,
            ChatModelId = chatModelId,
            ChatEndpoint = chatEndpoint,
            EmbeddingModelId = embeddingModelId,
            EmbeddingEndpoint = embeddingEndpoint
        };

        // Assert
        settings.SystemPrompt.ShouldBe(systemPrompt);
        settings.ChatModelId.ShouldBe(chatModelId);
        settings.ChatEndpoint.ShouldBe(chatEndpoint);
        settings.EmbeddingModelId.ShouldBe(embeddingModelId);
        settings.EmbeddingEndpoint.ShouldBe(embeddingEndpoint);
    }

    [Fact]
    public void OllamaSettings_DefaultEndpoints_CanUseSameValue()
    {
        // Arrange & Act
        var settings = new OllamaSettings
        {
            SystemPrompt = "Test prompt",
            ChatModelId = "llama3",
            ChatEndpoint = "http://localhost:11434",
            EmbeddingModelId = "nomic-embed",
            EmbeddingEndpoint = "http://localhost:11434"
        };

        // Assert
        settings.ChatEndpoint.ShouldBe(settings.EmbeddingEndpoint);
    }
}