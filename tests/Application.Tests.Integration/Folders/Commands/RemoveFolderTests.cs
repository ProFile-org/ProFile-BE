using Application.Common.Exceptions;
using Application.Folders.Commands;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Commands;

public class RemoveFolderTests : BaseClassFixture
{
    public RemoveFolderTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenFolderDoesNotExist()
    {
        // Arrange
        var command = new RemoveFolder.Command()
        {
            FolderId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Folder does not exist.");
    }
}