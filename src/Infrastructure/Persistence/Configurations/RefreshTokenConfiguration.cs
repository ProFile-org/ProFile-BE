using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Token);
        builder.Property(x => x.Token)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.JwtId)
            .IsRequired();

        builder.Property(x => x.CreationDateTime)
            .IsRequired();
        
        builder.Property(x => x.ExpiryDateTime)
            .IsRequired();
        
        builder.Property(x => x.IsUsed)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(x => x.IsInvalidated)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey("UserId")
            .IsRequired();
    }
}