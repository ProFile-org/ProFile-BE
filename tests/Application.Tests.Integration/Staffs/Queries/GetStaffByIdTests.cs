using Application.Identity;
using Application.Staffs.Queries;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Staffs.Queries;

public class GetStaffByIdTests : BaseClassFixture
{
    public GetStaffByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnStaff_WhenIdIsValid()
    {
        // Arrange
        var department = CreateDepartment();
        var user = CreateUser(IdentityData.Roles.Staff, "123123");
        var room = CreateRoom(department);
        var staff = CreateStaff(user, room);
        await AddAsync(staff);
        
        var query = new GetStaffById.Query()
        {   
            StaffId = staff.Id
        };
        
        // Act
        var result = await SendAsync(query);

        // Assert
        result.User.Id.Should().Be(staff.Id);

        // Cleanup
        Remove(staff);
        Remove(await FindAsync<Room>(room.Id));
        Remove(await FindAsync<User>(user.Id));
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenStaffDoesNotExist()
    {
        // Arrange
        var query = new GetStaffById.Query()
        {
            StaffId = Guid.NewGuid()
        };
        
        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Staff does not exist.");
    }
    
}