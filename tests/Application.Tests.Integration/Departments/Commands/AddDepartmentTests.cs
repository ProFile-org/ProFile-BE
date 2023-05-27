using Application.Common.Exceptions;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Departments.Commands;

public class AddDepartmentTests : BaseClassFixture 
{
    public AddDepartmentTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact(Timeout = 200)]
    public async Task ShouldCreateDepartment_WhenDepartmentNameIsValid()
    {
        // Arrange
        var createDepartmentCommand = _departmentGenerator.Generate();
        
        // Act
        var department = await SendAsync(createDepartmentCommand);
        
        // Assert
        department.Name.Should().Be(createDepartmentCommand.Name);
        
        // Cleanup
        var departmentEntity = await FindAsync<Department>(department.Id);
        Remove(departmentEntity);
    }

    [Fact]
    public async Task ShouldReturnConflict_WhenDepartmentNameHasExisted()
    {
        // Arrange
        var createDepartmentCommand = _departmentGenerator.Generate();
        var department = await SendAsync(createDepartmentCommand);
        
        // Act
        var action = async () => await SendAsync(createDepartmentCommand);
        
        // Assert
        await action.Should().ThrowAsync<ConflictException>().WithMessage("Department name already exists.");
        
        // Cleanup
        var departmentEntity = await FindAsync<Department>(department.Id);
        Remove(departmentEntity);
    }
}