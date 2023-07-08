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