using System.Data;
using Application.Helpers;
using Bogus;
using Dapper;
using Domain.Entities;
using NSubstitute;
using Xunit;

namespace Infrastructure.Tests.Unit.Repositories;

public class UserRepositoryTests
{
    private readonly UserRepository _sut;
    private readonly IDbConnection _connection = Substitute.For<IDbConnection>();
    private readonly IDbTransaction _transaction = Substitute.For<IDbTransaction>();


    private readonly Faker<Department> _departmentGenerator = new Faker<Department>()
        .RuleFor(x => x.Id, Guid.NewGuid())
        .RuleFor(x => x.Name, faker => faker.Commerce.Department());

    private readonly Faker<User> _userGenerator = new Faker<User>()
        .RuleFor(x => x.Username, faker => faker.Person.UserName)
        .RuleFor(x => x.Email, faker => faker.Person.Email)
        .RuleFor(x => x.FirstName, faker => faker.Person.FirstName)
        .RuleFor(x => x.LastName, faker => faker.Person.LastName)
        .RuleFor(x => x.Role, faker => faker.Random.Word())
        .RuleFor(x => x.Position, faker => faker.Random.Word())
        .RuleFor(x => x.PasswordHash, faker => faker.Random.Hash(64))
        .RuleFor(x => x.IsActive, faker => faker.Random.Bool())
        .RuleFor(x => x.IsActivated, false)
        .RuleFor(x => x.Created, DateTime.Now);

    public UserRepositoryTests()
    {
        _sut = new(_connection, _transaction);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUser_WhenCreateDetailsAreValid()
    {
    }
    // Arrange
        
    // Act
        
    // Assert
}