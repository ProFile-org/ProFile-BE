using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class ReportLostDocumentTests : BaseClassFixture
{
    public ReportLostDocumentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnBorrowRequest_WhenBorrowRequestStatusIsCheckedOut()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Borrowed;
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.CheckedOut);
        await AddAsync(borrow);
        
        var command = new ReportLostDocument.Command()
        {
            BorrowId = borrow.Id,
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Status.Should().Be(BorrowRequestStatus.Lost.ToString());
        var documentResult = await FindAsync<Document>(document.Id);
        documentResult?.Status.Should().Be(DocumentStatus.Lost);

        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(documentResult);
        Remove(user);
    }
    
    [Fact]
    public async Task ShouldReturnBorrowRequest_WhenBorrowRequestStatusIsOverdue()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Borrowed;
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Overdue);
        await AddAsync(borrow);
        
        var command = new ReportLostDocument.Command()
        {
            BorrowId = borrow.Id,
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Status.Should().Be(BorrowRequestStatus.Lost.ToString());
        var documentResult = await FindAsync<Document>(document.Id);
        documentResult?.Status.Should().Be(DocumentStatus.Lost);

        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(documentResult);
        Remove(user);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenBorrowRequestDoesNotExist()
    {
        // Arrange
        var command = new ReportLostDocument.Command()
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
    public async Task ShouldThrowConflictException_WhenDocumentStatusIsNotBorrowed()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Overdue);
        await AddAsync(borrow);
        
        var command = new ReportLostDocument.Command()
        {
            BorrowId = borrow.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<ConflictException>("Document is not borrowed.");

        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(document);
        Remove(user);
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenBorrowRequestStatusIsNotCheckedOutOrOverdue()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Borrowed;
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Approved);
        await AddAsync(borrow);
        
        var command = new ReportLostDocument.Command()
        {
            BorrowId = borrow.Id,
        };

        // Act
        var result = async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<ConflictException>("Request cannot be lost.");
        
        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(document);
        Remove(user);
    }
}