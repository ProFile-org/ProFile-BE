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
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(x => x.Email)
            .HasMaxLength(320)            
            .IsRequired();
        
        builder.Property(x => x.PasswordHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.PasswordSalt)
            .HasMaxLength(32)
            .IsRequired();
        
        builder.Property(x => x.FirstName)
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(x => x.LastName)
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey("DepartmentId")
            .IsRequired(false);
        
        builder.Property(x => x.Role)
            .HasMaxLength(64)
            .IsRequired();
        
        builder.Property(x => x.Position)
            .HasMaxLength(64)
            .IsRequired(false);
        
        builder.Property(x => x.IsActive)
            .IsRequired();
        
        builder.Property(x => x.IsActivated)
            .IsRequired();
        
        builder.Property(x => x.Created)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired(false);

        builder.Property(x => x.LastModified)
            .IsRequired(false);

        builder.Property(x => x.LastModifiedBy)
            .IsRequired(false);
    }
}