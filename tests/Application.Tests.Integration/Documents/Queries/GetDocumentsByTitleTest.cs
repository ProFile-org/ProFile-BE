using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Documents.Queries.GetDocumentsByTitle;
using Application.Identity;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Documents.Queries;

public class GetDocumentsByTitleTest : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetDocumentsByTitleTest(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<MappingProfile>());

        _mapper = configuration.CreateMapper();
    }
    
    [Fact]
    public async Task ShouldReturnAllDocuments_WhenUserRoleIsAdmin()
    {
        // Arrange
       
        var documents = CreateNDocuments(2);
        documents[0].Title = "doc1";
        documents[1].Title = "doc2";
        await AddAsync(documents[0]);
        await AddAsync(documents[1]);
        
        var query = new GetDocumentsByTitleQuery()
        {
            CurrentUserRole = IdentityData.Roles.Admin
        };

        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(2);
        
        // Cleanup
        Remove(documents[0]);
        Remove(documents[1]);
    }

    [Fact]
    public async Task ShouldReturnDocumentsOfUsersDepartment()
    {
        // Arrange
        var hrDepartment = CreateDepartment();
        var saleDepartment = CreateDepartment();
        
        var documents = CreateNDocuments(2);
        documents[0].Department = hrDepartment;
        documents[0].Title = "doc2";
        documents[1].Department = saleDepartment;
        documents[1].Title = "doc2";

        await AddAsync(documents[0]);
        await AddAsync(documents[1]);

        var query = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "doc",
            CurrentUserDepartment = hrDepartment.Name,
            CurrentUserRole = IdentityData.Roles.Staff
        };
        
        // Act 
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(1); 
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentDto>(documents.First()));

        // Cleanup
        Remove(documents[0]);
        Remove(documents[1]);
        Remove(hrDepartment);
        Remove(saleDepartment);
    }

    [Fact]
    public async Task ShouldReturnNoDocuments_WhenUserDepartmentIsNull()
    {
        // Arrange
        var documents = CreateNDocuments(1);
        documents.First().Title = "document";
        await AddAsync(documents.First());

        var query = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "doc",
            CurrentUserDepartment = null,
            CurrentUserRole = IdentityData.Roles.Staff
        };
        
        // Act
        var result = await SendAsync(query);

        // Assert
        result.TotalCount.Should().Be(0);

        // Cleanup
        Remove(documents.First());
    }

    [Fact]
    public async Task ShouldReturnAllDocumentsWithValidConstraint_WhenSearchTermIsNull()
    {
        // Arrange
        var department = CreateDepartment();
        var documents = CreateNDocuments(2);
        documents[0].Title = "doc1";
        documents[0].Department = department;
        documents[1].Title = "doc2";
        await AddAsync(documents[0]);
        await AddAsync(documents[1]);

        var query = new GetDocumentsByTitleQuery()
        {
            CurrentUserDepartment = department.Name,
            CurrentUserRole = IdentityData.Roles.Staff
        };
        
        // Act
        var result = await SendAsync(query);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DocumentDto>(documents.First()));

        // Cleanup
        Remove(documents[0]);
        Remove(documents[1]);
        Remove(department);
    }

    [Fact]
    public async Task ShouldReturnEmptyListOfDocuments_WhenThereAreNoDocumentsContainSearchTerm()
    {
        // Arrange
        var documents = CreateNDocuments(2);
        documents[0].Title = "doc1";
        documents[1].Title = "doc2";
        await AddAsync(documents[0]);
        await AddAsync(documents[1]);

        var query = new GetDocumentsByTitleQuery()
        {
            SearchTerm = "who am i",
            CurrentUserRole = IdentityData.Roles.Admin
        };

        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.TotalCount.Should().Be(0);
        
        // Cleanup
        Remove(documents[0]);
        Remove(documents[1]);
    }

}