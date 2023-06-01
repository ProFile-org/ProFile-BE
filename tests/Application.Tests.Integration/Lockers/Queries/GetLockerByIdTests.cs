using Application.Lockers.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Queries;

public class GetLockerByIdTests : BaseClassFixture
{
    public GetLockerByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldReturnLocker_WhenThatLockerExists()
    {
        // Arrange
        var department = CreateDepartment();
        var locker = CreateLocker();
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        
        var query = new GetLockerById.Query()
        {
            LockerId = locker.Id,
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Id.Should().Be(locker.Id);
        result.Name.Should().Be(locker.Name);

        // Cleanup
        Remove(locker);
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatLockerDoesNotExist()
    {
        // Arrange
        var query = new GetLockerById.Query()
        {
            LockerId = Guid.NewGuid(),
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }
}