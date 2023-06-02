using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Lockers.Queries;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Queries;

public class GetAllLockersPaginatedTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllLockersPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnLockers()
    {
        // Arrange
        var department = CreateDepartment();
        var locker1 = CreateLocker();
        var locker2 = CreateLocker();
        var room = CreateRoom(department, locker1, locker2);
        await AddAsync(room);

        var query = new GetAllLockersPaginated.Query();
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().BeEquivalentTo(_mapper.Map<LockerDto[]>(new[] { locker1, locker2 })
            .OrderBy(x => x.Id), config => config.IgnoringCyclicReferences());
        
        // Cleanup
        Remove(locker1);
        Remove(locker2);
        Remove(room);
        Remove(department);
    }
    
    [Fact]
    public async Task ShouldReturnLockersOfOneRoom_WhenSpecifyIdOfThatRoom()
    {
        // Arrange
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        var locker1 = CreateLocker();
        var locker2 = CreateLocker();
        var room1 = CreateRoom(department1, locker1, locker2);
        var locker3 = CreateLocker();
        var locker4 = CreateLocker();
        var room2 = CreateRoom(department2, locker3, locker4);
        await AddAsync(room1);
        await AddAsync(room2);

        var query = new GetAllLockersPaginated.Query()
        {
            RoomId = room1.Id,
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().ContainEquivalentOf(_mapper.Map<LockerDto>(locker1),
            config => config.IgnoringCyclicReferences());
        result.Items.Should().ContainEquivalentOf(_mapper.Map<LockerDto>(locker2),
            config => config.IgnoringCyclicReferences());
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<LockerDto>(locker3),
            config => config.IgnoringCyclicReferences());
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<LockerDto>(locker4),
            config => config.IgnoringCyclicReferences());
        
        // Cleanup
        Remove(locker1);
        Remove(locker2);
        Remove(room1);
        Remove(room2);
        Remove(department1);
        Remove(department2);
    }
}