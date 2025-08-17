using MattEland.Jaimes.Agents.Functions;
using MattEland.Jaimes.Agents.Messages;
using MattEland.Jaimes.Agents.Models;
using MattEland.Jaimes.Core.Domain;
using MattEland.Jaimes.Core.Helpers;
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
        Character character = new()
        {
            Name = "Test",
            Specialization = "Testing",
            CharacterSheet = "Stuff"
        };
        OrchestrationConfiguration configuration = new()
        {
            ModelServiceAssignments = new Dictionary<string, string>
            {
                { "Planner", "TestService" },
                { "Composer", "TestService" },
                { "Editor", "TestService" },
                { "Evaluator", "TestService" },
            }
        };
        ConversationMessage message = new()
        {
            History = history,
            Configuration = configuration,
            Adventure = new Adventure
            {
                Name = "Test",
                Author = "Test",
                Version = "1.0",
                Ruleset = "TEST",
                Backstory = "Boring",
                SettingDescription = "Awesome",
                LocationsOverview = "Nothing to see here",
                Locations = [],
                EncountersOverview = "Players gunna die",
                Encounters = [],
                GameMasterNotes = "Don't forget to have fun",
                NarrativeStructure = "Arrange / Act / Assert",
                Characters = [character],
                PlayerCharacter = character
            },
            Character = character
        };

        // Act
        PlanCompleteMessage response = await planner.GenerateAsync(message, configuration);

        // Assert
        response.ShouldNotBeNull();
        response.Plan.Cautions.ShouldBe(plan.Cautions);
        response.Plan.Checks.ShouldBe(plan.Checks);
        response.Plan.KeyPoints.Count.ShouldBe(plan.KeyPoints.Count);
        response.Plan.KeyPoints[0].ShouldBe(plan.KeyPoints[0]);
        Mock.Verify(chatClientMock);
    }
}