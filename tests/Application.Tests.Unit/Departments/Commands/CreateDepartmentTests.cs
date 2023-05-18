using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Departments.Commands.CreateDepartment;
using Application.Users.Queries;
using AutoMapper;
using Bogus;
using Domain.Entities;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace Application.Tests.Unit.Departments.Commands;

public class CreateDepartmentTests
{
    // private readonly CreateDepartmentCommandHandler _sut;
    // private readonly Faker<CreateDepartmentCommand> _departmentGenerator = new Faker<CreateDepartmentCommand>()
    //     .RuleFor(x => x.Name, faker => faker.Commerce.Department());
    // private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    // private readonly IMapper _mapper;
    // public CreateDepartmentTests()
    // {
    //     var configuration = new MapperConfiguration(config => 
    //         config.AddProfile<MappingProfile>());
    //     if (configuration == null) throw new ArgumentNullException(nameof(configuration));
    //     _mapper = configuration.CreateMapper();
    //     _sut = new(_uow, _mapper);
    // }
    //
    // [Fact]
    // public async Task ShouldCreateDepartment()
    // {
    //     // Arrange
    //     var department = _departmentGenerator.Generate();
    //     _uow.DepartmentRepository.GetByIdAsync(Arg.Any<Guid>())
    //         .ReturnsNull();
    //     var entity = new Department
    //     {
    //         Id = Guid.NewGuid(),
    //         Name = department.Name
    //     };
    //     _uow.DepartmentRepository.CreateDepartmentAsync(Arg.Is<Department>(d => d.Name.Equals(department.Name)))
    //         .Returns(entity);
    //     var expected = _mapper.Map<DepartmentDto>(entity);
    //
    //     // Act
    //     var result = await _sut.Handle(department, CancellationToken.None);
    //
    //     // Assert
    //     result.Should().BeEquivalentTo(expected);
    // }
}