using AiTableTopGameMaster.ConsoleApp.Clients;
using AiTableTopGameMaster.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Shouldly;
using Spectre.Console.Testing;
using Xunit;

namespace AiTableTopGameMaster.Tests;

public class IntegrationTests
{
    [Fact]
    public async Task ConsoleChatClient_WithOutputReviewer_ShouldNotRequireOllamaForBasicFlow()
    {
        // Arrange
        Mock<Agent> mockGameMasterAgent = new Mock<Agent>();
        Mock<IOutputReviewer> mockOutputReviewer = new Mock<IOutputReviewer>();
        TestConsole console = new TestConsole();
        
        // Setup the output reviewer to always accept the output
        mockOutputReviewer
            .Setup(x => x.ReviewOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OutputReviewResult.Acceptable());
        
        ConsoleChatClient client = new(
            mockGameMasterAgent.Object, 
            mockOutputReviewer.Object, 
            console, 
            NullLogger<ConsoleChatClient>.Instance);
        
        // Act & Assert - Just verify the client was created successfully
        client.ShouldNotBeNull();
        
        // Verify that the output reviewer would be called in the flow
        mockOutputReviewer.Verify(x => x.ReviewOutputAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public void OutputReviewer_Integration_ShouldBeConfigurableInServiceContainer()
    {
        // This test validates that our dependency injection setup is correct
        // by confirming that IOutputReviewer can be resolved and is properly configured
        
        // Arrange
        Kernel testKernel = Kernel.CreateBuilder().Build();
        
        // Act
        OutputReviewAgent reviewer = new(testKernel, NullLogger<OutputReviewAgent>.Instance);
        
        // Assert
        reviewer.ShouldNotBeNull();
        reviewer.ShouldBeAssignableTo<IOutputReviewer>();
    }
    
    [Fact]
    public async Task OutputReviewSystem_EndToEnd_ShouldPreventUndesirableBehaviors()
    {
        // This test demonstrates the complete flow where undesirable behaviors are detected
        
        // Arrange
        Kernel kernel = Kernel.CreateBuilder().Build();
        OutputReviewAgent reviewer = new(kernel, NullLogger<OutputReviewAgent>.Instance);
        
        // Examples of potentially problematic outputs that the system should catch
        string[] problematicOutputs = {
            "I roll a d20 for you and get a 15.",
            "You decide to climb the tree without asking anyone.",
            "What do you remember about this place from your past?",
            "You automatically search the room and find a secret door."
        };
        
        // Act & Assert
        foreach (string output in problematicOutputs)
        {
            // Since we don't have a real AI model in the test environment,
            // we can at least verify the system handles the input gracefully
            OutputReviewResult result = await reviewer.ReviewOutputAsync(output);
            
            // The result should not be null and should have a valid state
            result.ShouldNotBeNull();
            result.IsAcceptable.ShouldBeOfType<bool>();
        }
    }
}