using Application.Helpers;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Unit.Helpers;

public class SecurityUtilTests
{
    public SecurityUtilTests()
    {
    }

    [Fact]
    public void ShouldReturnHash_WhenHashPasswordWithSaltAndPepper()
    {
        // Arrange
        string salt = "dwnqjkdqwW4q";
        string input = "ThizIsAveRyl0000GandS3cur4dP@ssWord";
        string pepper = "Some secret here";
        string expectedHash = "27745bdd5e09aae12213a1ea7a3f9056b7de257050370d6cfa1c2d1b954de335";
        // Act
        string password = input.HashPasswordWith(salt, pepper);
        
        // Assert
        password.Should().Be(expectedHash);
    }
}