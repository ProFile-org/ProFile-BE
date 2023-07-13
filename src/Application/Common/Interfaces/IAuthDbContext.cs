using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IAuthDbContext
{
    public DbSet<RefreshToken> RefreshTokens { get;  }
    public DbSet<ResetPasswordToken> ResetPasswordTokens { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}