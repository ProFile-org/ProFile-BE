using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class UpdateBorrowTests : BaseClassFixture
{
    public UpdateBorrowTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldUpdateBorrow_WhenDetailsAreValid()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "aaaaaa");

        var document = CreateNDocuments(1).First();

        var borrow = CreateBorrowRequest(user, document, BorrowRequestStatus.Pending);

        await AddAsync(borrow);

        var command = new UpdateBorrow.Command()
        {
            BorrowReason = "Example Update",
            BorrowFrom = DateTime.Now.AddDays(3),
            BorrowTo = DateTime.Now.AddDays(12),
            BorrowId = borrow.Id,
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.BorrowReason.Should().Be(command.BorrowReason);
        result.BorrowTime.Should().Be(command.BorrowFrom);
        result.DueTime.Should().Be(command.BorrowTo);
        
        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(user);
        Remove(document);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRequestDoesNotExist()
    {
        // Arrange
        var command = new UpdateBorrow.Command()
        {
            BorrowReason = "adsda",
            BorrowFrom = DateTime.Now.AddHours(1),
            BorrowTo = DateTime.Now.AddHours(2),
            BorrowId = Guid.NewGuid(),
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Borrow request does not exist.");
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenRequestStatusIsNotPending()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "a");

        var document = CreateNDocuments(1).First();

        var borrow = CreateBorrowRequest(user, document, BorrowRequestStatus.Approved);

        await AddAsync(borrow);

        var command = new UpdateBorrow.Command()
        {
            BorrowReason = "Example Update",
            BorrowFrom = DateTime.Now.AddDays(3),
            BorrowTo = DateTime.Now.AddDays(12),
            BorrowId = borrow.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Cannot update borrow request.");
        
        // Cleanup
        Remove(borrow);
        Remove(user);
        Remove(document);
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenDocumentIsLost()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "a");

        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Lost;

        var borrow = CreateBorrowRequest(user, document, BorrowRequestStatus.Pending);

        await AddAsync(borrow);

        var command = new UpdateBorrow.Command()
        {
            BorrowReason = "Example Update",
            BorrowFrom = DateTime.Now.AddDays(3),
            BorrowTo = DateTime.Now.AddDays(12),
            BorrowId = borrow.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Document is lost.");
        
        // Cleanup
        Remove(borrow);
        Remove(user);
        Remove(document);
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenRequestTimespanOverlapAnApprovedOrCheckedOutRequestTimespan()
    {
        // Arrange
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
        var user1 = CreateUser(IdentityData.Roles.Employee, "a");
        var user2 = CreateUser(IdentityData.Roles.Employee, "a");

        var document = CreateNDocuments(1).First();

        var borrow1 = CreateBorrowRequest(user1, document, BorrowRequestStatus.Pending);
        var borrow2 = CreateBorrowRequest(user2, document, BorrowRequestStatus.Approved);
        borrow2.BorrowTime = LocalDateTime.FromDateTime(DateTime.Now.AddDays(4));
        borrow2.DueTime = LocalDateTime.FromDateTime(DateTime.Now.AddDays(12));

        await context.AddAsync(borrow1);
        await context.AddAsync(borrow2);

        await context.SaveChangesAsync();

        var command = new UpdateBorrow.Command()
        {
            BorrowReason = "Example Update",
            BorrowFrom = DateTime.Now.AddDays(5),
            BorrowTo = DateTime.Now.AddDays(13),
            BorrowId = borrow1.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("This document cannot be borrowed.");
        
        // Cleanup
        Remove(borrow1);
        Remove(borrow2);
        Remove(user1);
        Remove(user2);
        Remove(document);
    }
}