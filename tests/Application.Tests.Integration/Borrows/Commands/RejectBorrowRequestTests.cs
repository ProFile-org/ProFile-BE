using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class RejectBorrowRequestTests : BaseClassFixture
{
    public RejectBorrowRequestTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldRejectBorrowRequest_WhenRequestIsValid()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Pending);
        await AddAsync(borrow);
        var command = new RejectBorrowRequest.Command()
        {
            BorrowId = borrow.Id,
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Id.Should().Be(borrow.Id);
        result.Status.Should().Be(BorrowRequestStatus.Rejected.ToString());

        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(document);
        Remove(user);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenBorrowRequestDoesNotExist()
    {
        // Arrange
        var command = new RejectBorrowRequest.Command()
        {
            BorrowId = Guid.NewGuid(),
        };
        
        // Act
        var result = async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Borrow request does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenBorrowRequestIsNotPending()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Overdue);
        await AddAsync(borrow);
        var command = new RejectBorrowRequest.Command()
        {
            BorrowId = borrow.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Request cannot be rejected.");

        // Cleanup
        Remove(borrow);
        Remove(document);
        Remove(user);
    }
}