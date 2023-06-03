using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Common.Mappings;
using Application.Documents.Commands;
using AutoMapper;
using Domain.Entities.Physical;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.Documents.Commands;

public class BorrowDocumentTests : BaseClassFixture
{
    public BorrowDocumentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldCreateBorrowRequest_WhenBorrowDetailsAreValid()
    {
        // Arrange
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var department = CreateDepartment();
        var document = CreateNDocuments(1).First();
        var folder = CreateFolder(document);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        var user = CreateUser("Employee", "random");
        user.Department = department;
        await context.AddAsync(room);
        await context.AddAsync(user);
        await context.SaveChangesAsync();
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = document.Id,
            BorrowTo = DateTime.Now.AddDays(1),
            Reason = "khoa on ko?",
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Should().NotBeNull();
        result.BorrowerId.Should().Be(user.Id);
        result.DocumentId.Should().Be(document.Id);
        result.DueTime.Should().Be(command.BorrowTo);
        result.Reason.Should().Be(command.Reason);
        
        // Cleanup
        Remove(await FindAsync<Borrow>(result.Id));
        Remove(document);
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(user);
        Remove(department);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var department = CreateDepartment();
        var document = CreateNDocuments(1).First();
        var folder = CreateFolder(document);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        await AddAsync(room);
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = Guid.NewGuid(),
            DocumentId = document.Id,
            BorrowTo = DateTime.Now.AddDays(1),
            Reason = "khoa on ko?",
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User does not exist.");
        
        // Cleanup
        Remove(document);
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(department);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenDocumentDoesNotExist()
    {
        // Arrange
        var department = CreateDepartment();
        var user = CreateUser("Employee", "random");
        user.Department = department;
        await AddAsync(user);
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = Guid.NewGuid(),
            BorrowTo = DateTime.Now.AddDays(1),
            Reason = "khoa on ko?",
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Document does not exist.");
        
        // Cleanup
        Remove(user);
        Remove(department);
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenDueDateIsInThePast()
    {
        // Arrange
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var department = CreateDepartment();
        var document = CreateNDocuments(1).First();
        var folder = CreateFolder(document);
        var locker = CreateLocker(folder);
        var room = CreateRoom(department, locker);
        var user = CreateUser("Employee", "random");
        user.Department = department;
        await context.AddAsync(room);
        await context.AddAsync(user);
        await context.SaveChangesAsync();
        
        var command = new BorrowDocument.Command()
        {
            BorrowerId = user.Id,
            DocumentId = document.Id,
            BorrowTo = DateTime.Now.AddDays(-1),
            Reason = "khoa on ko?",
        };
        
        // Act
        var action = async () => await SendAsync(command);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Due date cannot be in the past.");
        
        // Cleanup
        Remove(document);
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(user);
        Remove(department);
    }
}