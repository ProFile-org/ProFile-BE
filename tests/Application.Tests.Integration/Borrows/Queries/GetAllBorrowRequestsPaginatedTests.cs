using Application.Borrows.Queries;
using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Identity;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Borrows.Queries;

public class GetAllBorrowRequestsPaginatedTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllBorrowRequestsPaginatedTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnBorrowRequests()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "password");
        var user2 = CreateUser(IdentityData.Roles.Employee, "password");
        var documents = CreateNDocuments(2);
        var borrowRequest1 = CreateBorrowRequest(user, documents[0], BorrowRequestStatus.Pending);
        var borrowRequest2 = CreateBorrowRequest(user2, documents[1], BorrowRequestStatus.Approved);
        await AddAsync(borrowRequest1);
        await AddAsync(borrowRequest2);

        var query = new GetAllBorrowRequestsPaginated.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().BeInAscendingOrder(x => x.Status);
        result.Items.Should().ContainEquivalentOf(_mapper.Map<BorrowDto>(borrowRequest1), 
            config => config
                .Excluding(x => x.DueTime)
                .Excluding(x => x.BorrowTime)
                .IgnoringCyclicReferences());
        
        result.Items.Should().ContainEquivalentOf(_mapper.Map<BorrowDto>(borrowRequest2), 
            config => config
                .Excluding(x => x.DueTime)
                .Excluding(x => x.BorrowTime)
                .IgnoringCyclicReferences());
                                                                                                                                                                
        // Cleanup
        Remove(borrowRequest2);
        Remove(borrowRequest1);
        Remove(await FindAsync<Document>(documents[0].Id));
        Remove(await FindAsync<Document>(documents[1].Id));
        Remove(await FindAsync<User>(user2.Id));
        Remove(await FindAsync<User>(user.Id));
    }

    [Fact]
    public async Task ShouldReturnSpecificBorrowRequest()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "password");
        var documents = CreateNDocuments(1);
        var borrowRequest = CreateBorrowRequest(user, documents[0], BorrowRequestStatus.Pending);
        await AddAsync(borrowRequest);
    
        var query = new GetAllBorrowRequestsPaginated.Query()
        {
            DocumentId = documents[0].Id
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.First().Should().BeEquivalentTo(_mapper.Map<BorrowDto>(borrowRequest), 
            config => config
                .Excluding(x => x.BorrowTime)
                .Excluding(x => x.DueTime));
        
        // Cleanup
        Remove(borrowRequest);
        Remove(await FindAsync<Document>(documents[0].Id));
        Remove(await FindAsync<User>(user.Id));
    }

    [Fact]
    public async Task ShouldReturnNothing_WhenThereAreNoBorrowRequest()
    {
        // Arrange
        var query = new GetAllBorrowRequestsPaginated.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
}