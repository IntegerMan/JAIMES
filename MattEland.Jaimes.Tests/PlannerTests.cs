using AiTableTopGameMaster.Core.Models;
using MattEland.Jaimes.Agents;
using MattEland.Jaimes.RAG;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Shouldly;
using FunctionResultContent = Microsoft.Extensions.AI.FunctionResultContent;

namespace MattEland.Jaimes.Tests;

public class PlannerTests
{
    [Fact]
    public async Task PlannerShouldCallExpectedServices()
    {
        // Arrange
        ModelInfo modelInfo = new()
        {
            Id = "planner-model",
            Provider = ModelProvider.Ollama,
            Type = ModelType.Chat,
            Endpoint = "http://localhost:11434",
            ModelId = "test-model",
            SupportsTools = true
        };

        ChatHistory history = new();
        Mock<ITranscriptService> transcriptServiceMock = new Mock<ITranscriptService>(MockBehavior.Strict);
        transcriptServiceMock
            .Setup(ts => ts.GetChatHistory())
            .Returns(history)
            .Verifiable(Times.Once);

        Mock<IChatCompletionService> chatClientMock = new Mock<IChatCompletionService>(MockBehavior.Strict);
        chatClientMock.Setup(m => m.GetChatMessageContentsAsync(It.IsAny<ChatHistory>(), null, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ChatMessageContent()
                {
                    InnerContent = new PlannerResponse()
                    {
                        Cautions = "Hey",
                        Checks = "None",
                        KeyPoints = "You Guys"
                    }
                }
            ])
            .Verifiable(Times.Once);

        Mock<IKernelBuilder> kernelBuilderMock = new Mock<IKernelBuilder>(MockBehavior.Strict);
        kernelBuilderMock.SetupGet(m => m.Services)
            .Returns(new ServiceCollection().AddScoped<IChatCompletionService>(_ => chatClientMock.Object));

        Kernel kernel = new Kernel(null, null);

        Mock<IModelFactory> modelFactory = new Mock<IModelFactory>(MockBehavior.Strict);
        modelFactory
            .Setup(mf => mf.ConfigureKernel(kernelBuilderMock.Object, "Planner", modelInfo, It.IsAny<string[]>()))
            .Verifiable(Times.Once);

        PlannerAgent planner = new(transcriptServiceMock.Object, modelFactory.Object, kernelBuilderMock.Object, modelInfo);

        // Act
        PlannerResponse response = await planner.GenerateAsync();

        // Assert
        response.ShouldNotBeNull();
        response.Cautions.ShouldBe("Hey");
        response.Checks.ShouldBe("None");
        response.KeyPoints.ShouldBe("You Guys");
        Mock.VerifyAll(transcriptServiceMock, modelFactory, kernelBuilderMock, chatClientMock);
    }
}