using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class CancelDocumentRequestTests : BaseClassFixture
{
    public CancelDocumentRequestTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnCanceledBorrowRequest_WhenStatusIsApproved()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        var borrow = CreateBorrowRequest(user,document,BorrowRequestStatus.Approved);
        await AddAsync(borrow);
        
        var command = new CancelBorrowRequest.Command()
        {
            BorrowId = borrow.Id
        };
        // Act
        var result = await SendAsync(command);
        // Assert
        borrow.Id.Should().Be(result.Id);
        result.Status.Should().Be(BorrowRequestStatus.Cancelled.ToString());
        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(document);
        Remove(user);
    }

    [Fact]
    public async Task ShouldReturnCanceledBorrowRequest_WhenStatusIsPending()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        var borrow = CreateBorrowRequest(user,document,BorrowRequestStatus.Pending);
        await AddAsync(borrow);
        
        var command = new CancelBorrowRequest.Command()
        {
            BorrowId = borrow.Id
        };
        // Act
        var result = await SendAsync(command);
        // Assert
        borrow.Id.Should().Be(result.Id);
        result.Status.Should().Be(BorrowRequestStatus.Cancelled.ToString());
        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(document);
        Remove(user);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenBorrowRequestDoesNotExist()
    {
        // Arrange
        var command = new CancelBorrowRequest.Command()
        {
            BorrowId = Guid.NewGuid()
        };
        // Act
        var result = async () => await SendAsync(command);
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Borrow request does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenBorrowRequestIsNotApprovedOrPending()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Lost);
        await AddAsync(borrow);
        var command = new CancelBorrowRequest.Command()
        {
            BorrowId = borrow.Id
        };
        // Act
        var result = async () => await SendAsync(command);
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Request cannot be cancelled.");
        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(document);
        Remove(user);
    }
}