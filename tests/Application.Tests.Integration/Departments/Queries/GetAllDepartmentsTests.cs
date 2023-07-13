using Application.Common.Mappings;
using Application.Common.Models.Dtos;
using Application.Departments.Queries;
using Application.Users.Queries;
using AutoMapper;
using Bogus;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Departments.Queries;

public class GetAllDepartmentsTests : BaseClassFixture
{
    private readonly IMapper _mapper;
    public GetAllDepartmentsTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        var configuration = new MapperConfiguration(x => x.AddProfile<MappingProfile>());
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public async Task ShouldReturnDepartments_WhenDepartmentsExist()
    {
        // Arrange
        var department = new Department()
        {
            Id = Guid.NewGuid(),
            Name = new Faker().Commerce.Department()
        };
        await AddAsync(department);
        var query = new GetAllDepartments.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Should().ContainEquivalentOf(_mapper.Map<DepartmentDto>(department));
        
        // Cleanup
        Remove(department);
    }
    
    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoDepartmentsExist()
    {
        // Arrange
        var query = new GetAllDepartments.Query();

        // Act
        var result = await SendAsync(query);

        // Assert
        result.Items.Count().Should().Be(0);
    }
}