using Application.Common.Exceptions;
using Application.Folders.Commands;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class UpdateFolderTests : BaseClassFixture
{
    public UpdateFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatFolderDoesNotExist()
    {
        // Arrange
        var command = new UpdateFolder.Command()
        {
            FolderId = Guid.NewGuid(),
            Name = "Something else",
            Capacity = 6,
            Description = "ehehe",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Folder does not exist.");
    }
}