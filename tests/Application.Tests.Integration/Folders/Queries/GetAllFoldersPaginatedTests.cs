using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Folders.Queries;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Folders.Queries;

public class GetAllFoldersPaginatedTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllFoldersPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnFolders()
    {
        // Arrange
        var department = CreateDepartment();
        var folder1 = CreateFolder();
        var folder2 = CreateFolder();
        var folder3 = CreateFolder();
        var locker1 = CreateLocker(folder1);
        var locker2 = CreateLocker(folder2, folder3);
        var room = CreateRoom(department, locker1, locker2);
        await AddAsync(room);

        var query = new GetAllFoldersPaginated.Query();
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().BeEquivalentTo(_mapper.Map<FolderDto[]>(new[] { folder1, folder2, folder3 })
            .OrderBy(x => x.Id), config => config.IgnoringCyclicReferences());
        result.Items.Should().BeInAscendingOrder(x => x.Id);
        
        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(folder3);
        Remove(locker1);
        Remove(locker2);
        Remove(room);
        Remove(department);
    }
    
    [Fact]
    public async Task ShouldReturnFoldersOfASpecificLocker()
    {
        // Arrange
        var department = CreateDepartment();
        var folder1 = CreateFolder();
        var folder2 = CreateFolder();
        var folder3 = CreateFolder();
        var locker1 = CreateLocker(folder1);
        var locker2 = CreateLocker(folder2, folder3);
        var room = CreateRoom(department, locker1, locker2);
        await AddAsync(room);

        var query = new GetAllFoldersPaginated.Query()
        {
            RoomId = room.Id,
            LockerId = locker2.Id,
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().BeEquivalentTo(_mapper.Map<FolderDto[]>(new[] { folder2, folder3 })
            .OrderBy(x => x.Id), config => config.IgnoringCyclicReferences());
        result.Items.Should().BeInAscendingOrder(x => x.Id);
        
        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(folder3);
        Remove(locker1);
        Remove(locker2);
        Remove(room);
        Remove(department);
    }
    
    [Fact]
    public async Task ShouldReturnNothing_WhenWrongPaginationDetailsAreProvided()
    {
        // Arrange
        var department = CreateDepartment();
        var folder1 = CreateFolder();
        var folder2 = CreateFolder();
        var folder3 = CreateFolder();
        var locker1 = CreateLocker(folder1);
        var locker2 = CreateLocker(folder2, folder3);
        var room = CreateRoom(department, locker1, locker2);
        await AddAsync(room);

        var query = new GetAllFoldersPaginated.Query()
        {
            RoomId = room.Id,
            LockerId = locker2.Id,
            Page = -1,
            Size = -4,
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().BeEquivalentTo(_mapper.Map<FolderDto[]>(new[] { folder2, folder3 })
            .OrderBy(x => x.Id), config => config.IgnoringCyclicReferences());
        result.Items.Should().BeInAscendingOrder(x => x.Id);
        
        // Cleanup
        Remove(folder1);
        Remove(folder2);
        Remove(folder3);
        Remove(locker1);
        Remove(locker2);
        Remove(room);
        Remove(department);
    }
}