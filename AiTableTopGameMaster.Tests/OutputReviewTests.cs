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
}