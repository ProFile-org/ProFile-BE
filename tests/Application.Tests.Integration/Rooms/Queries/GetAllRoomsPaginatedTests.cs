using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Rooms.Queries;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Rooms.Queries;

public class GetAllRoomsPaginatedTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllRoomsPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnAllRooms()
    {
        // Arrange
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        var room1 = CreateRoom(department1);
        var room2 = CreateRoom(department2);
        await AddAsync(room1);
        await AddAsync(room2);
        
        var query = new GetAllRoomsPaginated.Query();
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should()
            .ContainEquivalentOf(_mapper.Map<RoomDto>(room1),
                config => config.IgnoringCyclicReferences());
        result.Items.Should()
            .ContainEquivalentOf(_mapper.Map<RoomDto>(room2),
                config => config.IgnoringCyclicReferences());
        
        // Cleanup
        Remove(room1);
        Remove(room2);
        Remove(department1);
        Remove(department2);
    }
    
    [Fact]
    public async Task ShouldReturnOrderById_WhenWrongSortByIsProvided()
    {
        // Arrange
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        var room1 = CreateRoom(department1);
        var room2 = CreateRoom(department2);
        await AddAsync(room1);
        await AddAsync(room2);
        
        var query = new GetAllRoomsPaginated.Query()
        {
            SortBy = "e",
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<RoomDto[]>(new[] { room1, room2 })
                .OrderBy(x => x.Id), config => config.IgnoringCyclicReferences());
        result.Items.Should().BeInAscendingOrder(x => x.Id);
        
        // Cleanup
        Remove(room1);
        Remove(room2);
        Remove(department1);
        Remove(department2);
    }
}