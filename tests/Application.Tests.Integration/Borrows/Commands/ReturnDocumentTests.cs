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
    public async Task ShouldReturnTheDocument_WhenDocumentIsCheckedOut()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "abcdef");

        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Borrowed;

        var borrow = CreateBorrowRequest(user, document, BorrowRequestStatus.CheckedOut);

        await AddAsync(borrow);

        var command = new ReturnDocument.Command()
        {
            DocumentId = document.Id,
        };
        
        // Act
        var result = await SendAsync(command);
        var documentResult = await FindAsync<Document>(document.Id);

        // Assert
        result.Status.Should().Be(BorrowRequestStatus.Returned.ToString());
        result.ActualReturnTime.Should().BeBefore(result.DueTime);
        documentResult!.Status.Should().Be(DocumentStatus.Available);
        
        // Cleanup
        Remove(borrow);
        Remove(user);
        Remove(document);
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
        await result.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Borrow request does not exist.");
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
        await result.Should().ThrowAsync<ConflictException>().WithMessage("Document is not borrowed.");
        
        // Cleanup
        Remove(borrow);
        Remove(user);
        Remove(document);
    }
}