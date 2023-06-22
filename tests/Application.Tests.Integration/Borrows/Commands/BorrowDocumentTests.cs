using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Entities;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class BorrowDocumentTests : BaseClassFixture
{
    public BorrowDocumentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldCreateBorrowRequest_WhenDetailsAreValid()
    {
        // Arrange
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var department = CreateDepartment();
        await context.AddAsync(department);
        
        var user = CreateUser(IdentityData.Roles.Employee, "aaaaaa");
        user.Department = department;
        await context.AddAsync(user);

        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        document.Department = department;
        await context.AddAsync(document);
        await context.SaveChangesAsync();

        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = document.Id,
            BorrowReason = "Example",
            BorrowFrom = DateTime.Now.Add(TimeSpan.FromHours(1)),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(1)),
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.DocumentId.Should().Be(command.DocumentId);
        result.BorrowerId.Should().Be(command.BorrowerId);
        result.BorrowReason.Should().Be(command.BorrowReason);
        result.BorrowTime.Should().Be(command.BorrowFrom);
        result.DueTime.Should().Be(command.BorrowTo);
        result.Status.Should().Be(BorrowRequestStatus.Pending.ToString());
        
        // Cleanup
        Remove(await FindAsync<Borrow>(result.Id));
        Remove(user);
        Remove(document);
        Remove(await FindAsync<Department>(department.Id));
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        await AddAsync(document);
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = Guid.NewGuid(),
            DocumentId = document.Id,
            BorrowFrom = DateTime.Now.Add(TimeSpan.FromDays(1)),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(2)),
            BorrowReason = "Example",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User does not exist.");
        
        // Cleanup
        Remove(document);
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenUserIsNotActive()
    {
        // Arrange
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        await AddAsync(document);
        
        var user = CreateUser(IdentityData.Roles.Employee, "aaaaaa");
        user.IsActive = false;
        await AddAsync(user);
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = document.Id,
            BorrowFrom = DateTime.Now.Add(TimeSpan.FromDays(1)),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(2)),
            BorrowReason = "Example",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("User is not active.");
        
        // Cleanup
        Remove(document);
        Remove(user);
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenUserIsNotActivated()
    {
        // Arrange
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        await AddAsync(document);
        
        var user = CreateUser(IdentityData.Roles.Employee, "aaaaaa");
        user.IsActivated = false;
        await AddAsync(user);
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = document.Id,
            BorrowFrom = DateTime.Now.Add(TimeSpan.FromDays(1)),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(2)),
            BorrowReason = "Example",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("User is not activated.");
        
        // Cleanup
        Remove(document);
        Remove(user);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenDocumentDoesNotExist()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "bbbbbb");
        await AddAsync(user);

        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = Guid.NewGuid(),
            BorrowFrom = DateTime.Now.Add(TimeSpan.FromDays(1)),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(2)),
            BorrowReason = "Example",
        };

        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Document does not exist.");
        
        // Cleanup
        Remove(user);
    }
    
    [Fact]
    public async Task ShouldConflictException_WhenDocumentIsNotAvailable()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "bbbbbb");
        await AddAsync(user);

        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Borrowed;
        await AddAsync(document);

        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = document.Id,
            BorrowFrom = DateTime.Now.Add(TimeSpan.FromDays(1)),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(2)),
            BorrowReason = "Example",
        };

        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Document is not available.");
        
        // Cleanup
        Remove(user);
        Remove(document);
    }
    
    [Fact]
    public async Task ShouldConflictException_WhenUserAndDocumentDoesNotBelongToTheSameDepartment()
    {
        // Arrange
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var department1 = CreateDepartment();
        var department2 = CreateDepartment();
        
        var user = CreateUser(IdentityData.Roles.Employee, "bbbbbb");
        user.Department = department1;
        await context.AddAsync(user);

        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        document.Department = department2;
        await context.AddAsync(document);

        await context.SaveChangesAsync();

        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = document.Id,
            BorrowFrom = DateTime.Now.Add(TimeSpan.FromDays(1)),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(2)),
            BorrowReason = "Example",
        };

        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("User is not allowed to borrow this document.");
        
        // Cleanup
        Remove(user);
        Remove(document);
        Remove(department1);
        Remove(department2);
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenRequestWithSameUserAndDocumentAlreadyExists()
    {
        // Arrange
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var department = CreateDepartment();
        
        var user = CreateUser(IdentityData.Roles.Employee, "bbbbbb");
        user.Department = department;
        
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        document.Department = department;

        var borrow = CreateBorrowRequest(user, document, BorrowRequestStatus.Pending);
        await context.AddAsync(borrow);
        
        await context.SaveChangesAsync();
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = document.Id,
            BorrowFrom = DateTime.Now.Add(TimeSpan.FromDays(1)),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(2)),
            BorrowReason = "Example",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("This document is already requested borrow from the same user.");
        
        // Cleanup;
        Remove(borrow);
        Remove(user);
        Remove(document);
        Remove(department);
    }

    [Fact] 
    public async Task ShouldThrowConflictException_WhenARequestIsMadeWhileDocumentIsAlreadyBeingBorrowed()
    {
        // Arrange
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var department = CreateDepartment();
        
        var user1 = CreateUser(IdentityData.Roles.Employee, "bbbbbb");
        user1.Department = department;
        
        var user2 = CreateUser(IdentityData.Roles.Employee, "bbbbbb");
        user2.Department = department;
        await context.AddAsync(user2);
        
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        document.Department = department;

        var borrow = CreateBorrowRequest(user1, document, BorrowRequestStatus.Approved);
        await context.AddAsync(borrow);
        
        await context.SaveChangesAsync();
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = user2.Id,
            DocumentId = document.Id,
            BorrowFrom = DateTime.Now.AddHours(1),
            BorrowTo = DateTime.Now.Add(TimeSpan.FromDays(2)),
            BorrowReason = "Example",
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("This document cannot be borrowed.");
        
        // Cleanup;
        Remove(borrow);
        Remove(user1);
        Remove(user2);
        Remove(document);
        Remove(department);
    }
}