using Application.Documents.Queries.GetDocumentsByTitle;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Documents.Queries;

public class GetDocumentsByTitleTest : BaseClassFixture
{
    public GetDocumentsByTitleTest(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldReturnAllDocuments_WhenUserRoleIsAdmin()
    {
        // Arrange
        var adminDepartment = CreateDepartment();
        var saleDepartment = CreateDepartment();

        var admin = CreateUser(adminDepartment);
        admin.Role = "Admin";
        await AddAsync(admin);
        
        var documents = CreateNDocuments(2);
        documents[0].Department = null;
        documents[0].Title = "doc1";

        documents[1].Department = saleDepartment;
        documents[1].Title = "doc2";

        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(locker);
        await AddAsync(room);

        var query = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "doc"
        };

        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.First().Department.Should().BeNull();
        
        // Cleanup
        Remove(documents[0]);
        Remove(documents[1]);
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(admin);
        Remove(adminDepartment);
        Remove(saleDepartment);
    }

    [Fact]
    public async Task ShouldReturnDocumentsOfUsersDepartment()
    {
        // Arrange
        var saleDepartment = CreateDepartment();
        var hrDepartment = CreateDepartment();

        var sale = CreateUser(saleDepartment);
        await AddAsync(sale);
        
        var documents = CreateNDocuments(1);
        documents.First().Department = hrDepartment;
        documents.First().Title = "document";

        var folder = CreateFolder(documents);
        var locker = CreateLocker(folder);
        var room = CreateRoom(locker);
        await AddAsync(room);

        var query = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "doc",
        };
        
        // Act 
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(0);
                
        // Cleanup
        Remove(documents.First());
        Remove(folder);
        Remove(locker);
        Remove(room);
        Remove(sale);
        Remove(saleDepartment);
        Remove(hrDepartment);
    }
    
}