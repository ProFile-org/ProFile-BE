using Application.Common.Mappings;
using Application.Common.Models.Dtos.Digital;
using Application.UserGroups.Queries;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.UserGroups.Queries;

public class GetAllUserGroupsPaginatedTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllUserGroupsPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(x => x.AddProfile<MappingProfile>());
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnUserGroups_WhenUserGroupsExist()
    {
        // Arrange
        var userGroup1 = CreateUserGroup(Array.Empty<User>());
        var userGroup2 = CreateUserGroup(Array.Empty<User>());
        
        await AddAsync(userGroup1);
        await AddAsync(userGroup2);
        var query = new GetAllUserGroupsPaginated.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<UserGroupDto[]>(new [] {userGroup1, userGroup2}));
        
        // Cleanup
        Remove(userGroup1);
        Remove(userGroup2);
    }
    
    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoUserGroupsExist()
    {
        // Arrange
        var query = new GetAllUserGroupsPaginated.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.TotalCount.Should().Be(0);
    }
    
    [Fact]
    public async Task ShouldReturnAscendingOrderById_WhenWrongSortByIsProvided()
    {
        // Arrange
        var userGroup1 = CreateUserGroup(Array.Empty<User>());
        var userGroup2 = CreateUserGroup(Array.Empty<User>());
        
        await AddAsync(userGroup1);
        await AddAsync(userGroup2);
        var query = new GetAllUserGroupsPaginated.Query()
        {
            SortBy = "example"
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<UserGroupDto[]>(new[] { userGroup1, userGroup2 })
                .OrderBy(x => x.Id));
        result.Items.Should().BeInAscendingOrder(x => x.Id);
        
        // Cleanup
        Remove(userGroup1);
        Remove(userGroup2);
    }
    
    [Fact]
    public async Task ShouldReturnAscendingOrderByName_WhenSearchTermIsProvided()
    {
        // Arrange
        var userGroup1 = CreateUserGroup(Array.Empty<User>());
        var userGroup2 = CreateUserGroup(Array.Empty<User>());
        var userGroup3 = CreateUserGroup(Array.Empty<User>());
        userGroup3.Name = userGroup1.Name + "2";
        
        await AddAsync(userGroup1);
        await AddAsync(userGroup2);
        await AddAsync(userGroup3);
        var query = new GetAllUserGroupsPaginated.Query()
        {
            SearchTerm = userGroup1.Name
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<UserGroupDto[]>(new[] { userGroup1, userGroup3 })
                .OrderBy(x => x.Id));
        result.Items.Should().BeInAscendingOrder(x => x.Id);
        
        // Cleanup
        Remove(userGroup1);
        Remove(userGroup2);
        Remove(userGroup3);
    }
    
    [Fact]
    public async Task ShouldReturnSortByInAscendingOrder_WhenCorrectSortByIsProvided()
    {
        // Arrange
        var userGroup1 = CreateUserGroup(Array.Empty<User>());
        var userGroup2 = CreateUserGroup(Array.Empty<User>());
        
        await AddAsync(userGroup1);
        await AddAsync(userGroup2);
        var query = new GetAllUserGroupsPaginated.Query()
        {
            SortBy = "Name"
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<UserGroupDto[]>(new[] { userGroup1, userGroup2 })
                .OrderBy(x => x.Name));
        result.Items.Should().BeInAscendingOrder(x => x.Name);
        
        // Cleanup
        Remove(userGroup1);
        Remove(userGroup2);
    }

    [Fact]
    public async Task ShouldReturnInDescendingOrderOfId_WhenDescendingSortOrderIsProvided()
    {
        // Arrange
        var userGroup1 = CreateUserGroup(Array.Empty<User>());
        var userGroup2 = CreateUserGroup(Array.Empty<User>());
        
        await AddAsync(userGroup1);
        await AddAsync(userGroup2);
        var query = new GetAllUserGroupsPaginated.Query()
        {
            SortOrder = "desc"
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should()
            .BeEquivalentTo(_mapper.Map<UserGroupDto[]>(new[] { userGroup1, userGroup2 })
                .OrderByDescending(x => x.Id));
        result.Items.Should().BeInDescendingOrder(x => x.Id);
        
        // Cleanup
        Remove(userGroup1);
        Remove(userGroup2);
    }
}