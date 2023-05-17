using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(320);
        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(64);
        builder.Property(x => x.FirstName)
            .HasMaxLength(50);
        builder.Property(x => x.LastName)
            .HasMaxLength(50);
        builder.Property(x => x.Role)
            .HasMaxLength(64);
        builder.Property(x => x.Position)
            .HasMaxLength(64);
        builder.Property(x => x.IsActive)
            .IsRequired();
        builder.Property(x => x.IsActivated)
            .IsRequired();
        builder.Property(x => x.Created)
            .IsRequired();
    }
}