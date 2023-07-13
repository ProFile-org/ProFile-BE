using Application.Folders.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Queries;

public class GetFolderByIdTests : BaseClassFixture
{
    public GetFolderByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatFolderDoesNotExist()
    {
        // Arrange
        var query = new GetFolderById.Query()
        {
            FolderId = Guid.NewGuid(),
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Folder does not exist.");
    }
}