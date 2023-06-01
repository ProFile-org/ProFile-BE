using Application.Departments.Queries;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Departments.Queries;

public class GetDepartmentByIdTests : BaseClassFixture
{
    public GetDepartmentByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task ShouldReturnDepartment_WhenThatDepartmentExists()
    {
        // Arrange
        var department = CreateDepartment();

        await AddAsync(department);

        var query = new GetDepartmentById.Query()
        {
            DepartmentId = department.Id,
        };
        
        // Act
        var result = await SendAsync(query);
        
        // Assert
        result.Name.Should().Be(department.Name);
        
        // Cleanup
        Remove(department);
    }

    [Fact]
    public async Task ShouldThrowNotFoundException_WhenThatDepartmentDoesNotExist()
    {
        // Arrange
        var query = new GetDepartmentById.Query()
        {
            DepartmentId = Guid.NewGuid(),
        };
        
        // Act
        var action = async () => await SendAsync(query);
        
        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Department does not exist.");
    }
}