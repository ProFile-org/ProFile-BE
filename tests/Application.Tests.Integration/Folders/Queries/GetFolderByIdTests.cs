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
    public async Task ShouldReturnFolder_WhenThatFolderExists()
    {
        // Arrange
        var department = CreateDepartment();
        var folder = CreateFolder();
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        
        var query = new GetFolderById.Query()
        {
            FolderId = folder.Id,
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Id.Should().Be(folder.Id);
        result.Name.Should().Be(folder.Name);

        // Cleanup
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
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