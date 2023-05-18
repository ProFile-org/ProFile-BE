using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Departments.Commands;

public class CreateDepartmentTests : BaseClassFixture 
{
    public CreateDepartmentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact(Timeout = 1000)]
    public async Task ShouldCreateDepartment()
    {
        // Arrange
        var createDepartmentCommand = _departmentGenerator.Generate();
        
        // Act
        var department = await SendAsync(createDepartmentCommand);
        
        // Assert
        department.Name.Should().Be(createDepartmentCommand.Name);
    }
}