using Domain.Entities;
using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.HasAlternateKey(x => x.Name);
        builder.Property(x => x.Name)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.HasOne(x => x.Department)
            .WithOne(x => x.Room)
            .HasForeignKey<Department>(x => x.RoomId)
            .IsRequired(false);

        builder.Property(x => x.Capacity)
            .IsRequired();
        
        builder.Property(x => x.NumberOfLockers)
            .IsRequired();
        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.IsAvailable)
            .IsRequired();
    }
}