using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class CheckoutDocumentTest : BaseClassFixture
{
    public CheckoutDocumentTest(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldCheckoutDocument_WhenBorrowRequestDocument()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Approved);
        await AddAsync(borrow);

        var command = new CheckoutDocument.Command()
        {
            BorrowId = borrow.Id
        };
        
        // Act
        var result = await SendAsync(command);

        // Assert
        borrow.Id.Should().Be(result.Id);
        result.Status.Should().Be(BorrowRequestStatus.CheckedOut.ToString());
        var documentResult = await FindAsync<Document>(result.DocumentId);
        documentResult?.Status.Should().Be(DocumentStatus.Borrowed);

        // Cleanup
        Remove(await FindAsync<Borrow>(borrow.Id));
        Remove(documentResult);
        Remove(user);
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenBorrowRequestDoesNotExist()
    {
        // Arrange
        var command = new CheckoutDocument.Command()
        {
            BorrowId = Guid.NewGuid(),
        };

        // Act
        var result =async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Borrow request does not exist.");

    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenDocumentStatusIsNotAvailable()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Borrowed;
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Approved);
        await AddAsync(borrow);

        var command = new CheckoutDocument.Command()
        {
            BorrowId = borrow.Id
        };
        
        // Act
        var result = async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Document is not available.");

        // Cleanup
        Remove(borrow);
        Remove(document);
        Remove(user);
    }

    [Fact]
    public async Task ShouldThrowConflictException_WhenBorrowRequestStatusIsApproved()
    {
        // Arrange
        var user = CreateUser(IdentityData.Roles.Employee, "randompassword");
        var document = CreateNDocuments(1).First();
        document.Status = DocumentStatus.Available;
        var borrow = CreateBorrowRequest(user,document, BorrowRequestStatus.Cancelled);
        await AddAsync(borrow);

        var command = new CheckoutDocument.Command()
        {
            BorrowId = borrow.Id
        };
        
        // Act
        var result = async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<ConflictException>()
            .WithMessage("Request cannot be checked out.");

        // Cleanup
        Remove(borrow);
        Remove(document);
        Remove(user);
    }
}