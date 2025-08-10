using AiTableTopGameMaster.Core.Plugins.Sourcebooks;
using Shouldly;

namespace AiTableTopGameMaster.Tests.Core.Plugins.Sourcebooks;

public class IndexingInfoTests
{
    [Theory]
    [InlineData("/path/to/document.pdf", "document_id", true)]
    [InlineData("/another/path/file.pdf", "another_id", false)]
    [InlineData("", "", true)]
    [InlineData("local/file.pdf", "local_file", false)]
    public void IndexingInfo_Initialization_SetsPropertiesCorrectly(string location, string documentId, bool isComplete)
    {
        // Arrange & Act
        var indexingInfo = new IndexingInfo(location, documentId, isComplete);

        // Assert
        indexingInfo.Location.ShouldBe(location);
        indexingInfo.DocumentId.ShouldBe(documentId);
        indexingInfo.IsComplete.ShouldBe(isComplete);
    }

    [Fact]
    public void IndexingInfo_AsRecord_SupportsValueEquality()
    {
        // Arrange
        var info1 = new IndexingInfo("/path/to/doc.pdf", "doc_id", true);
        var info2 = new IndexingInfo("/path/to/doc.pdf", "doc_id", true);
        var info3 = new IndexingInfo("/path/to/doc.pdf", "doc_id", false);

        // Act & Assert
        info1.ShouldBe(info2); // Records with same values are equal
        info1.ShouldNotBe(info3); // Records with different values are not equal
        info1.GetHashCode().ShouldBe(info2.GetHashCode()); // Hash codes match for equal records
    }

    [Fact]
    public void IndexingInfo_WithExpression_CreatesNewInstanceWithUpdatedProperties()
    {
        // Arrange
        var originalInfo = new IndexingInfo("/path/to/doc.pdf", "original_id", false);

        // Act
        var updatedInfo = originalInfo with { IsComplete = true };
        var updatedDocumentId = originalInfo with { DocumentId = "new_id" };

        // Assert
        updatedInfo.Location.ShouldBe(originalInfo.Location);
        updatedInfo.DocumentId.ShouldBe(originalInfo.DocumentId);
        updatedInfo.IsComplete.ShouldBeTrue();

        updatedDocumentId.Location.ShouldBe(originalInfo.Location);
        updatedDocumentId.DocumentId.ShouldBe("new_id");
        updatedDocumentId.IsComplete.ShouldBe(originalInfo.IsComplete);

        // Original should remain unchanged
        originalInfo.IsComplete.ShouldBeFalse();
        originalInfo.DocumentId.ShouldBe("original_id");
    }

    [Fact]
    public void IndexingInfo_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var indexingInfo = new IndexingInfo("/test/path.pdf", "test_id", true);

        // Act
        var result = indexingInfo.ToString();

        // Assert
        result.ShouldContain("/test/path.pdf");
        result.ShouldContain("test_id");
        result.ShouldContain("True");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IndexingInfo_IsCompleteProperty_ReflectsIndexingStatus(bool isComplete)
    {
        // Arrange & Act
        var indexingInfo = new IndexingInfo("/path/to/doc.pdf", "doc_id", isComplete);

        // Assert
        indexingInfo.IsComplete.ShouldBe(isComplete);
    }

    [Fact]
    public void IndexingInfo_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var indexingInfo = new IndexingInfo("/test/document.pdf", "test_document", true);

        // Act
        var (location, documentId, isComplete) = indexingInfo;

        // Assert
        location.ShouldBe("/test/document.pdf");
        documentId.ShouldBe("test_document");
        isComplete.ShouldBeTrue();
    }
}