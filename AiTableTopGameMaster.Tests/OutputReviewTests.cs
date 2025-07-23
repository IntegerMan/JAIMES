using AiTableTopGameMaster.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using Shouldly;
using Xunit;

namespace AiTableTopGameMaster.Tests;

public class OutputReviewTests
{
    [Fact]
    public void OutputReviewResult_Acceptable_ShouldCreateAcceptableResult()
    {
        // Act
        OutputReviewResult result = OutputReviewResult.Acceptable();
        
        // Assert
        result.IsAcceptable.ShouldBeTrue();
        result.Feedback.ShouldBeNull();
        result.Issues.ShouldBeEmpty();
    }
    
    [Fact]
    public void OutputReviewResult_NeedsRevision_ShouldCreateRevisionResult()
    {
        // Arrange
        const string feedback = "Test feedback";
        const string issue1 = "Issue 1";
        const string issue2 = "Issue 2";
        
        // Act
        OutputReviewResult result = OutputReviewResult.NeedsRevision(feedback, issue1, issue2);
        
        // Assert
        result.IsAcceptable.ShouldBeFalse();
        result.Feedback.ShouldBe(feedback);
        result.Issues.ShouldContain(issue1);
        result.Issues.ShouldContain(issue2);
        result.Issues.Count.ShouldBe(2);
    }
    
    [Fact]
    public async Task OutputReviewAgent_ReviewOutputAsync_EmptyInput_ShouldReturnAcceptable()
    {
        // Arrange
        Kernel kernel = Kernel.CreateBuilder().Build();
        OutputReviewAgent agent = new(kernel, NullLogger<OutputReviewAgent>.Instance);
        
        // Act
        OutputReviewResult result = await agent.ReviewOutputAsync("");
        
        // Assert
        result.IsAcceptable.ShouldBeTrue();
    }
    
    [Fact]
    public async Task OutputReviewAgent_ReviewOutputAsync_WhitespaceInput_ShouldReturnAcceptable()
    {
        // Arrange
        Kernel kernel = Kernel.CreateBuilder().Build();
        OutputReviewAgent agent = new(kernel, NullLogger<OutputReviewAgent>.Instance);
        
        // Act
        OutputReviewResult result = await agent.ReviewOutputAsync("   \n\t  ");
        
        // Assert
        result.IsAcceptable.ShouldBeTrue();
    }
    
    [Theory]
    [InlineData("The goblin attacks! You need to roll for initiative.")]
    [InlineData("Make a Perception check to see if you notice anything unusual.")]
    [InlineData("You explore the room carefully, looking for clues about what happened here.")]
    public async Task OutputReviewAgent_ParseReviewResponse_AcceptableResponse_ShouldReturnAcceptable(string response)
    {
        // Arrange
        Kernel kernel = Kernel.CreateBuilder().Build();
        OutputReviewAgent agent = new(kernel, NullLogger<OutputReviewAgent>.Instance);
        
        // Use reflection to test the private ParseReviewResponse method
        var method = typeof(OutputReviewAgent).GetMethod("ParseReviewResponse", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act
        OutputReviewResult result = (OutputReviewResult)method.Invoke(agent, new object[] { "ACCEPTABLE" });
        
        // Assert
        result.IsAcceptable.ShouldBeTrue();
    }
    
    [Theory]
    [InlineData("NEEDS_REVISION The response contains dice rolling on behalf of the player")]
    [InlineData("NEEDS_REVISION You are asking the player what their character remembers")]
    [InlineData("NEEDS_REVISION Taking actions for the player without their input")]
    public async Task OutputReviewAgent_ParseReviewResponse_NeedsRevisionResponse_ShouldReturnNeedsRevision(string response)
    {
        // Arrange
        Kernel kernel = Kernel.CreateBuilder().Build();
        OutputReviewAgent agent = new(kernel, NullLogger<OutputReviewAgent>.Instance);
        
        // Use reflection to test the private ParseReviewResponse method
        var method = typeof(OutputReviewAgent).GetMethod("ParseReviewResponse", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act
        OutputReviewResult result = (OutputReviewResult)method.Invoke(agent, new object[] { response });
        
        // Assert
        result.IsAcceptable.ShouldBeFalse();
        result.Feedback.ShouldNotBeNull();
        result.Issues.ShouldNotBeEmpty();
    }
    
    [Fact]
    public async Task OutputReviewAgent_ParseReviewResponse_UnexpectedFormat_ShouldReturnAcceptable()
    {
        // Arrange
        Kernel kernel = Kernel.CreateBuilder().Build();
        OutputReviewAgent agent = new(kernel, NullLogger<OutputReviewAgent>.Instance);
        
        // Use reflection to test the private ParseReviewResponse method
        var method = typeof(OutputReviewAgent).GetMethod("ParseReviewResponse", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act
        OutputReviewResult result = (OutputReviewResult)method.Invoke(agent, new object[] { "Some unexpected response format" });
        
        // Assert
        result.IsAcceptable.ShouldBeTrue();
    }
}