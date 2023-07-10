using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using Application.Staffs.Queries;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Staffs.Queries;

public class GetAllStaffsPaginatedTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllStaffsPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnStaffs()
    {
        // Arrange
        var user1 = CreateUser(IdentityData.Roles.Staff, "123123");
        var user2 = CreateUser(IdentityData.Roles.Staff, "435455");
        var staff1 = CreateStaff(user1, null);
        var staff2 = CreateStaff(user2, null);
        await AddAsync(staff1);
        await AddAsync(staff2);

        var query = new GetAllStaffsPaginated.Query();

        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);

        // Cleanup
        Remove(staff2);
        Remove(staff1);
        Remove(await FindAsync<User>(user2.Id));
        Remove(await FindAsync<User>(user1.Id));
    }

    [Fact]
    public async Task ShouldReturnASpecificStaff()
    {
        // Arrange
        var user1 = CreateUser(IdentityData.Roles.Staff, "123123");
        var user2 = CreateUser(IdentityData.Roles.Staff, "435455");
        var staff1 = CreateStaff(user1, null);
        var staff2 = CreateStaff(user2, null);
        await AddAsync(staff1);
        await AddAsync(staff2);
        
        var query = new GetAllStaffsPaginated.Query()
        {
            SearchTerm = staff1.User.Username
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(1);
        
        // Cleanup
        Remove(staff2);
        Remove(staff1);
        Remove(await FindAsync<User>(user2.Id));
        Remove(await FindAsync<User>(user1.Id));
    }
}