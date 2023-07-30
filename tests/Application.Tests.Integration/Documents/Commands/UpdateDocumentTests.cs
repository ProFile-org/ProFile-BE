using Application.Common.Exceptions;
using Application.Documents.Commands;
using Application.Identity;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.Documents.Commands;

public class UpdateDocumentTests : BaseClassFixture
{
    public UpdateDocumentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatDocumentDoesNotExist()
    {
        // Arrange
        var command = new UpdateDocument.Command()
        {
            DocumentId = Guid.NewGuid(),
            Title = "Khoa is ngu",
            Description = "This would probably not be duplicated",
            DocumentType = "Hehehe",
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Document does not exist.");
    }
}