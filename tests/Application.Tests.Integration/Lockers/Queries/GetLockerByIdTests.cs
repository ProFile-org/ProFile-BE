using Application.Lockers.Queries;
using Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Integration.Lockers.Queries;

public class GetLockerByIdTests : BaseClassFixture
{
    public GetLockerByIdTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenThatLockerDoesNotExist()
    {
        // Arrange
        var query = new GetLockerById.Query()
        {
            LockerId = Guid.NewGuid(),
        };

        // Act
        var action = async () => await SendAsync(query);

        // Assert
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Locker does not exist.");
    }
}