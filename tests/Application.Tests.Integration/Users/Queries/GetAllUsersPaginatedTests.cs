using Application.Common.Mappings;
using Application.Identity;
using Application.Users.Queries;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Users.Queries;

public class GetAllUsersPaginatedTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllUsersPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnUsers()
    {
        // Arrange
        var user1 = CreateUser(IdentityData.Roles.Admin, "admin");
        var user2 = CreateUser(IdentityData.Roles.Employee, "employee");
        await AddAsync(user1);
        await AddAsync(user2);

        var query = new GetAllUsersPaginated.Query();

        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainEquivalentOf(_mapper.Map<UserDto>(user2), config => config.Excluding(x => x.Created));
        result.Items.Should().NotContainEquivalentOf(_mapper.Map<UserDto>(user1), config => config.Excluding(x => x.Created));
        
        // Cleanup
        Remove(user1);
        Remove(user2);
    }
}