using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Staffs.Queries;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Staffs.Queries;

public class GetStaffByRoomTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    
    public GetStaffByRoomTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());
        _mapper = configuration.CreateMapper();
    }
    
    [Fact]
    public async Task ShouldReturnStaff_WhenRoomHaveStaff()
    {
        // Arrange
        var department = CreateDepartment();
        var room = CreateRoom(department);
        var user = CreateUser(IdentityData.Roles.Staff, "123123");
        var staff = CreateStaff(user, room);
        await AddAsync(staff);

        var query = new GetStaffByRoom.Query()
        {
            RoomId = room.Id 
        };

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Should().BeEquivalentTo(_mapper.Map<StaffDto>(staff), 
            config => config.Excluding(x => x.User.Created));
        result.User.Should().BeEquivalentTo(_mapper.Map<UserDto>(user), 
            config => config.Excluding(x => x.Created));
        result.Room.Should().BeEquivalentTo(_mapper.Map<RoomDto>(room));

        // Cleanup
        Remove(staff);
        Remove(await FindAsync<User>(user.Id));
        Remove(await FindAsync<Room>(room.Id));
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange 
        var query = new GetStaffByRoom.Query()
        {
            RoomId = Guid.NewGuid()
        };
        
        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist.");
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotHaveStaff()
    {
        // Arrange 
        var department = CreateDepartment();
        var room = CreateRoom(department);
        await AddAsync(room);

        var query = new GetStaffByRoom.Query()
        {
            RoomId = room.Id
        };
        
        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Staff does not exist.");
        
        // Cleanup
        Remove(room);
        Remove(await FindAsync<Department>(department.Id));
    }
}