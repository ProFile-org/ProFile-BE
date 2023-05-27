using Application.Documents.Queries.GetDocumentTypes;
using Bogus;
using Domain.Entities.Physical;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Documents.Queries;

public class GetAllDocumentTypesTests : BaseClassFixture
{
    public GetAllDocumentTypesTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnDocumentTypes_WhenDocumentTypesExist()
    {
        // Arrange
        var document = new Document()
        {
            Id = Guid.NewGuid(),
            Title = new Faker().Name.JobTitle(),
            DocumentType = new Faker().Commerce.ProductName(),
        };
        await AddAsync(document);
        var query = new Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Should().Contain(document.DocumentType);
        
        // Cleanup
        Remove(document);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoDocumentTypesExist()
    {
        // Arrange
        var query = new Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Should().BeEmpty();
    }
}