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

public class ApproveBorrowRequestTests : BaseClassFixture
{
    public ApproveBorrowRequestTests(CustomApiFactory apiFactory) : base(apiFactory)
    {
        
    }

    [Fact]
    public async Task ShouldThrowKeyNotFoundException_WhenRequestDoesNotExist()
    {
        // Arrange
        var command = new ApproveOrRejectBorrowRequest.Command()
        {
            BorrowId = Guid.NewGuid(),
        };
        
        // Act
        var result = async () => await SendAsync(command);

        // Assert
        await result.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Borrow request does not exist.");
    }
}