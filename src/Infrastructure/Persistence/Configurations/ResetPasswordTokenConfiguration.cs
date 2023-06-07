using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ResetPasswordTokenConfiguration : IEntityTypeConfiguration<ResetPasswordToken>
{
    public void Configure(EntityTypeBuilder<ResetPasswordToken> builder)
    {
        builder.HasKey(x => x.TokenHash);

        builder.Property(x => x.ExpirationDate)
            .IsRequired();
        
        builder.Property(x => x.IsInvalidated)
            .HasDefaultValue(false);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey()
            .IsRequired();
    }
}