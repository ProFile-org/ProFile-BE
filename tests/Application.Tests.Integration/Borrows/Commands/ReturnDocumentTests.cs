using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class ReturnDocumentTests : BaseClassFixture
{
    public ReturnDocumentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnBorrowRequest_WhenRequestIsValidAndBorrowRequestStatusIsCheckedOut()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "abcdef");

        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Borrowed;
        var borrow = CreateBorrowRequest(user, document, BorrowRequestStatus.CheckedOut);
        await AddAsync(borrow);
        var command = new ReturnDocument.Command()
        {
            DocumentId = document.Id
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Id.Should().Be(borrow.Id);
        result.Status.Should().Be(BorrowRequestStatus.Returned.ToString());
        var documentResult = await FindAsync<Document>(result.DocumentId);
        documentResult!.Status.Should().Be(DocumentStatus.Available);
        
        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(documentResult);
        Remove(user);
    }

    [Fact]
    public async Task ShouldReturnBorrowRequest_WhenRequestIsValidAndBorrowRequestStatusIsOverdue()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Borrowed;
        var borrow = CreateBorrowRequest(user,document,BorrowRequestStatus.Overdue);
        await AddAsync(borrow);
        var command = new ReturnDocument.Command()
        {
            DocumentId = document.Id
        };
        
        // Act
        var result = await SendAsync(command);
        
        // Assert
        result.Id.Should().Be(borrow.Id);
        result.Status.Should().Be(BorrowRequestStatus.Returned.ToString());
        var documentResult = await FindAsync<Document>(result.DocumentId);
        documentResult!.Status.Should().Be(DocumentStatus.Available);

        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(documentResult);
        Remove(user);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenBorrowRequestDoesNotExist()
    {
        // Arrange
        var command = new ReturnDocument.Command()
        {
            DocumentId = Guid.NewGuid(),
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
        var user = CreateUser(IdentityData.Roles.Employee, "abcdef");

        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;

        var borrow = CreateBorrowRequest(user, document, BorrowRequestStatus.CheckedOut);

        await AddAsync(borrow);

        var command = new ReturnDocument.Command()
        {
            DocumentId = document.Id,
        };
        
        // Act
        var result = async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Document is not borrowed.");

        // Cleanup
        Remove(borrow);
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
        var borrow = CreateBorrowRequest(user,document,BorrowRequestStatus.Rejected);
        await AddAsync(borrow);
        var command = new ReturnDocument.Command()
        {
            DocumentId = document.Id
        };
        
        // Act
        var result = async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Request cannot be made.");

        // Cleanup
        Remove(borrow);
        Remove(document);
        Remove(user);
    }
}