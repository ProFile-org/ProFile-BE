using Application.Common.Exceptions;
using Application.Documents.Commands;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Documents.Commands;

public class UpdateDocumentTests : BaseClassFixture
{
    public UpdateDocumentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldUpdateDocument_WhenUpdateDetailsAreValid()
    {
        // Arrange
        var document = CreateNDocuments(1).First();
        await AddAsync(document);
        
        var query = new UpdateDocument.Command()
        {
            DocumentId = document.Id,
            Title = "Khoa is ngu",
            Description = "This would probably not be duplicated",
            DocumentType = "Hehehe",
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Id.Should().Be(query.DocumentId);
        result.Title.Should().Be(query.Title);
        result.Description.Should().Be(query.Description);
        result.DocumentType.Should().Be(query.DocumentType);

        // Cleanup
        Remove(document);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatDocumentDoesNotExist()
    {
        // Arrange
        var query = new UpdateDocument.Command()
        {
            DocumentId = Guid.NewGuid(),
            Title = "Khoa is ngu",
            Description = "This would probably not be duplicated",
            DocumentType = "Hehehe",
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Document does not exist.");
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenNewDocumentTitleAlreadyExistsForTheImporter()
    {
        // Arrange
        var document = CreateNDocuments(1).First();
        
        await AddAsync(document);
        var query = new UpdateDocument.Command()
        {
            DocumentId = Guid.NewGuid(),
            Title = document.Title,
            Description = "This would probably not be duplicated",
            DocumentType = "Hehehe",
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Document name already exists for this importer.");
    }
}