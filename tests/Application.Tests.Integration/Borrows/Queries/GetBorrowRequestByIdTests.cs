using Application.Borrows.Queries;
using Application.Identity;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Borrows.Queries;

public class GetBorrowRequestByIdTests : BaseClassFixture
{
    public GetBorrowRequestByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnBorrowRequest_WhenBorrowRequsetExist()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "password");
        var documents = CreateNDocuments(1);
        var borrowRequest = CreateBorrowRequest(user, documents[0], BorrowRequestStatus.Approved);
        await AddAsync(borrowRequest);
        
        var query = new GetBorrowRequestById.Query()
        {
            User = user,
            BorrowId = borrowRequest.Id
        };
        
        // Act
        var result = await SendAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(borrowRequest.Id);
        result.BorrowerId.Should().Be(user.Id);

        // Cleanup
        Remove(borrowRequest);
        Remove(await FindAsync<Document>(documents[0].Id));
        Remove(await FindAsync<User>(user.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenBorrowRequestDoesNotExist()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "password");

        var query = new GetBorrowRequestById.Query()
        {
            User = user,
            BorrowId = Guid.NewGuid()
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Borrow request does not exist.");
    }

    [Fact]
    public async Task ShouldThrowUnauthorizedAccessException_WhenRequestUserIdIsInvalid()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "password");
        var user2 = CreateUser(IdentityData.Roles.Employee, "password");
        var documents = CreateNDocuments(1);
        var borrowRequest = CreateBorrowRequest(user, documents[0], BorrowRequestStatus.Approved);
        await AddAsync(borrowRequest);

        var query = new GetBorrowRequestById.Query()
        {
            User = user2,
            BorrowId = borrowRequest.Id
        };
        
        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<UnauthorizedAccessException>();

        // Cleanup
        Remove(borrowRequest);
        Remove(await FindAsync<Document>(documents[0].Id));
        Remove(await FindAsync<User>(user.Id));
    }
}