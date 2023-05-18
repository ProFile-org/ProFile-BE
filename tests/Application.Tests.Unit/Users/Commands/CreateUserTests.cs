using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Departments.Commands.CreateDepartment;
using Application.Helpers;
using Application.Users.Commands.CreateUser;
using Application.Users.Queries;
using AutoMapper;
using Bogus;
using Domain.Entities;
using FluentAssertions;
using Microsoft.VisualBasic;
using NSubstitute;
using Xunit;

namespace Application.Tests.Unit.Users.Commands;

public class CreateUserTests
{
    private readonly CreateUserCommandHandler _sut;
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper;
    private readonly Faker<CreateUserCommand> _userGenerator = new Faker<CreateUserCommand>()
        .RuleFor(x => x.Username, faker => faker.Person.UserName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FirstName, faker => faker.Person.FirstName)
        .RuleFor(x => x.LastName, faker => faker.Person.LastName)
        .RuleFor(x => x.Password, faker => faker.Random.String())
        .RuleFor(x => x.Role, faker => faker.Random.Word())
        .RuleFor(x => x.Position, faker => faker.Random.Word());

    public CreateUserTests()
    {
        var configuration = new MapperConfiguration(config => 
            config.AddProfile<MappingProfile>());
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        _mapper = configuration.CreateMapper();
        _sut = new(_uow, _mapper);
    }

    [Fact]
    public async Task ShouldCreateUser_WhenCreateDetailsAreValid()
    {
        // Arrange
        var id = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var departmentEntity = new Department
        {
            Id = departmentId,
            Name = ""
        };
        var request = _userGenerator.Generate();
        request = request with
        {
            DepartmentId = departmentId
        };
        var entity = new User
        {
            Id = id,
            Username = request.Username,
            PasswordHash = SecurityUtil.Hash(request.Password),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = departmentEntity,
            Role = request.Role,
            Position = request.Position,
            IsActive = true,
            IsActivated = false,
            Created = DateTime.Now
        };
        _uow.DepartmentRepository.GetByIdAsync(departmentId)
            .Returns(departmentEntity);
        _uow.UserRepository.CreateUserAsync(Arg.Is<User>(u =>
            u.Id == entity.Id
            && u.Username.Equals(entity.Username)
            && u.PasswordHash.Equals(entity.PasswordHash)
            && u.Email.Equals(entity.Email)
            && u.FirstName.Equals(entity.FirstName)
            && u.LastName.Equals(entity.LastName)
            && u.Role.Equals(entity.Role)
            && u.Position.Equals(entity.Position)
            && u.IsActivated == entity.IsActivated
            && u.IsActive == entity.IsActive
        )).Returns(entity);
        var expected = _mapper.Map<UserDto>(entity);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expected);

    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFound_WhenDepartmentDoesNotExist()
    {
        
    }
}