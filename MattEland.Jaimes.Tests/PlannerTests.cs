using MattEland.Jaimes.Agents.Definitions;
using MattEland.Jaimes.Agents.Functions;
using MattEland.Jaimes.Core.Helpers;
using MattEland.Jaimes.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Shouldly;

namespace MattEland.Jaimes.Tests;

public class PlannerTests
{
    [Fact]
    public async Task PlannerShouldCallExpectedServices()
    {
        // Arrange
        ChatHistory history = new();

        Mock<IChatCompletionService> chatClientMock = new Mock<IChatCompletionService>();
        PlannerResponse plan = new()
        {
            Cautions = "Hey",
            Checks = "None",
            KeyPoints = ["You Guys"]
        };
        chatClientMock.Setup(m => m.GetChatMessageContentsAsync(It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                plan.ToChatMessageContent(),
            ])
            .Verifiable(Times.Once);

        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddScoped<IChatCompletionService>(_ => chatClientMock.Object);
        Kernel kernel = kernelBuilder.Build();
        PlannerAgent planner = new(kernel);

        // Act
        (PlannerResponse response, _) = await planner.GenerateAsync(history);

        // Assert
        response.ShouldNotBeNull();
        response.Cautions.ShouldBe(plan.Cautions);
        response.Checks.ShouldBe(plan.Checks);
        response.KeyPoints.Count.ShouldBe(plan.KeyPoints.Count);
        response.KeyPoints[0].ShouldBe(plan.KeyPoints[0]);
        Mock.VerifyAll(chatClientMock);
    }
}