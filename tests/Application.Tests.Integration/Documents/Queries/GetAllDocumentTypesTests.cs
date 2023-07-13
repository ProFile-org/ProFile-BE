using Application.Documents.Queries;
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
        var document = CreateNDocuments(1).First();
        await AddAsync(document);
        var query = new GetAllDocumentTypes.Query();

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
        var query = new GetAllDocumentTypes.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Should().BeEmpty();
    }
}