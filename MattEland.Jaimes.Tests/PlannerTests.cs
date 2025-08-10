using AiTableTopGameMaster.Core.Helpers;
using AiTableTopGameMaster.Core.Models;
using MattEland.Jaimes.Agents;
using MattEland.Jaimes.Agents.Planner;
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

        Mock<IChatCompletionService> chatClientMock = new Mock<IChatCompletionService>(MockBehavior.Strict);
        PlannerResponse plan = new PlannerResponse()
        {
            Cautions = "Hey",
            Checks = "None",
            KeyPoints = "You Guys"
        };
        chatClientMock.Setup(m => m.GetChatMessageContentsAsync(It.IsAny<ChatHistory>(), null, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                plan.ToChatMessageContent(),
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

        PlannerAgent planner = new(modelFactory.Object, kernelBuilderMock.Object, modelInfo);

        // Act
        PlannerResponse response = await planner.GenerateAsync(history);

        // Assert
        response.ShouldNotBeNull();
        response.Cautions.ShouldBe(plan.Cautions);
        response.Checks.ShouldBe(plan.Checks);
        response.KeyPoints.ShouldBe(plan.KeyPoints);
        Mock.VerifyAll(modelFactory, kernelBuilderMock, chatClientMock);
    }
}