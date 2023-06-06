using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Statuses;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class ApproveBorrowRequestTests : BaseClassFixture
{
    public ApproveBorrowRequestTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldApproveRequest_WhenRequestIsValid()
    {
        // Arrange
        var document = CreateNDocuments(1).First();

        var user = CreateUser(IdentityData.Roles.Employee, "abcdef");

        var request = CreateBorrowRequest(user, document, BorrowRequestStatus.Pending);

        await AddAsync(request);
        
        var command = new ApproveBorrowRequest.Command()
        {
            BorrowId = request.Id,
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert 
        result.Status.Should().Be(BorrowRequestStatus.Approved.ToString());
        
        // Cleanup
        Remove(request);
        Remove(user);
        Remove(document);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRequestDoesNotExist()
    {
        // Arrange
        var command = new ApproveBorrowRequest.Command()
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
    public async Task ShouldThrowConflictException_WhenDocumentIsLost()
    {
        // Arrange
        var document = CreateNDocuments(1).First();

        document.Status = DocumentStatus.Lost;

        var user = CreateUser(IdentityData.Roles.Employee, "abcdef");

        var request = CreateBorrowRequest(user, document, BorrowRequestStatus.Pending);

        await AddAsync(request);
        
        var command = new ApproveBorrowRequest.Command()
        {
            BorrowId = request.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Document is lost. Request is unprocessable.");
        
        // Cleanup
        Remove(request);
        Remove(user);
        Remove(document);
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenRequestStatusIsNotPendingAndRejected()
    {
        var document = CreateNDocuments(1).First();

        var user = CreateUser(IdentityData.Roles.Employee, "abcdef");

        var request = CreateBorrowRequest(user, document, BorrowRequestStatus.CheckedOut);

        await AddAsync(request);
        
        var command = new ApproveBorrowRequest.Command()
        {
            BorrowId = request.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Request cannot be approved.");
        
        // Cleanup
        Remove(request);
        Remove(user);
        Remove(document);
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenRequestTimespanOverlapAnApprovedOrCheckedOutRequestTimespan()
    {
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var document = CreateNDocuments(1).First();

        var user1 = CreateUser(IdentityData.Roles.Employee, "abcdef");
        var user2 = CreateUser(IdentityData.Roles.Employee, "aaaaaa");

        var request1 = CreateBorrowRequest(user1, document, BorrowRequestStatus.Approved);
        
        var request2 = CreateBorrowRequest(user2, document, BorrowRequestStatus.Pending);
        request2.BorrowTime = request2.BorrowTime.Plus(Period.FromHours(1));
        request2.BorrowTime = request2.BorrowTime.Plus(Period.FromHours(1));
        
        await context.AddAsync(request1);
        await context.AddAsync(request2);
        await context.SaveChangesAsync();
        
        var command = new ApproveBorrowRequest.Command()
        {
            BorrowId = request2.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("This document cannot be borrowed.");
        
        // Cleanup
        Remove(request1);
        Remove(request2);
        Remove(user1);
        Remove(user2);
        Remove(document);
    }
}