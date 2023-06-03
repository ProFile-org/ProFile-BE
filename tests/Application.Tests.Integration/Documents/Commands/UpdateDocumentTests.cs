using Application.Common.Exceptions;
using Application.Documents.Commands;
using Application.Identity;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Tests.Integration.Documents.Commands;

public class UpdateDocumentTests : BaseClassFixture
{
    public UpdateDocumentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldUpdateDocument_WhenUpdateDetailsAreValid()
    {
        // Arrange
        var importer = CreateUser(IdentityData.Roles.Employee, "A RANDOM PASS");
        var document = CreateNDocuments(1).First();
        document.Importer = importer;
        await AddAsync(document);
        
        var command = new UpdateDocument.Command()
        {
            DocumentId = document.Id,
            Title = "Khoa is ngu",
            Description = "This would probably not be duplicated",
            DocumentType = "Hehehe",
        };

        // Act
        var result = await SendAsync(command);

        // Assert
        result.Id.Should().Be(command.DocumentId);
        result.Title.Should().Be(command.Title);
        result.Description.Should().Be(command.Description);
        result.DocumentType.Should().Be(command.DocumentType);

        // Cleanup
        Remove(document);
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatDocumentDoesNotExist()
    {
        // Arrange
        var command = new UpdateDocument.Command()
        {
            DocumentId = Guid.NewGuid(),
            Title = "Khoa is ngu",
            Description = "This would probably not be duplicated",
            DocumentType = "Hehehe",
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Document does not exist.");
    }
    
    [Fact]
    public async Task ShouldThrowConflictException_WhenNewDocumentTitleAlreadyExistsForTheImporter()
    {
        // Arrange
        using var scope = ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var importer = CreateUser(IdentityData.Roles.Employee, "A RANDOM PASS");
        var documentList = CreateNDocuments(2);

        var document1 = documentList[0];
        document1.Importer = importer;
        await context.AddAsync(document1);
        
        var document2 = documentList[1];
        document2.Importer = document1.Importer;
        document2.Title = "Another title";
        await context.AddAsync(document2);
        await context.SaveChangesAsync();
        
        var command = new UpdateDocument.Command()
        {
            DocumentId = document1.Id,
            Title = document2.Title,
            Description = "This would probably not be duplicated",
            DocumentType = "Hehehe",
        };

        // Act
        var action = async () => await SendAsync(command);

        // Assert
        await action.Should().ThrowAsync<ConflictException>();
        
        // Clean up
        Remove(document1);
        Remove(document2);
        Remove(importer);
    }
    
}