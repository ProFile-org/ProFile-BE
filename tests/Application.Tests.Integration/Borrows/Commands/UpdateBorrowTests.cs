using Application.Borrows.Commands;
using Application.Common.Exceptions;
using Application.Identity;
using Domain.Entities.Physical;
using Domain.Statuses;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Application.Tests.Integration.Borrows.Commands;

public class UpdateBorrowTests : BaseClassFixture
{
    public UpdateBorrowTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }
    
    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRequestDoesNotExist()
    {
        // Arrange
        var command = new UpdateBorrow.Command()
        {
            BorrowReason = "adsda",
            BorrowFrom = DateTime.Now.AddHours(1),
            BorrowTo = DateTime.Now.AddHours(2),
            BorrowId = Guid.NewGuid(),
        };
        
        // Act
        var result = async () => await SendAsync(command);
        
        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Borrow request does not exist.");
    }
}