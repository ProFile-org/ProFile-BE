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
    public async Task ShouldThrowKeyNotFoundException_WhenRoomDoesNotExist()
    {
        // Arrange 
        var query = new GetStaffByRoomId.Query()
        {
            RoomId = Guid.NewGuid()
        };
        
        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Room does not exist.");
    }
}