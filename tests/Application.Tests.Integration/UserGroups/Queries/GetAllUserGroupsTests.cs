using Application.Common.Mappings;
using Application.Common.Models.Dtos.Digital;
using Application.UserGroups.Queries;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.UserGroups.Queries;

public class GetAllUserGroupsTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllUserGroupsTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(x => x.AddProfile<MappingProfile>());
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnUserGroups_WhenUserGroupsExist()
    {
        // Arrange
        var userGroup = CreateUserGroup(Array.Empty<User>());
        
        await AddAsync(userGroup);
        var query = new GetAllUserGroups.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Should().ContainEquivalentOf(_mapper.Map<UserGroupDto>(userGroup));
        
        // Cleanup
        Remove(userGroup);
    }
    
    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoUserGroupsExist()
    {
        // Arrange
        var query = new GetAllUserGroups.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Count().Should().Be(0);
    }
}