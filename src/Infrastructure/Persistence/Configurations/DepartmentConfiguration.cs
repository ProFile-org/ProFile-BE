using Domain.Entities;
using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder.HasAlternateKey(x => x.Name);
        builder.Property(x => x.Name)
            .HasMaxLength(64);

        builder.HasOne(x => x.Room)
            .WithOne(x => x.Department)
            .HasForeignKey<Room>(x => x.DepartmentId)
            .IsRequired(false);
    }
}