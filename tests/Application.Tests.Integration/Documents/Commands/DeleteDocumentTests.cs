using Application.Documents.Commands;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Documents.Commands;

public class DeleteDocumentTests : BaseClassFixture
{
    public DeleteDocumentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldDeleteDocument_WhenThatDocumentExists()
    {
        // Arrange
        var document = CreateNDocuments(1).First();
        await AddAsync(document);

        var command = new DeleteDocument.Command()
        {
            DocumentId = document.Id,
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Id.Should().Be(document.Id);
        result.Title.Should().Be(document.Title);
        var deletedDocument = await FindAsync<Document>(document.Id);
        deletedDocument.Should().BeNull();
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatDocumentDoesNotExist()
    {
        // Arrange
        var command = new DeleteDocument.Command()
        {
            DocumentId = Guid.NewGuid(),
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Document does not exist.");
    }
}