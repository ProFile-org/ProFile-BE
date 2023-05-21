using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class LockerConfiguration : IEntityTypeConfiguration<Locker>
{
    public void Configure(EntityTypeBuilder<Locker> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Description)
            .HasMaxLength(256)
            .IsRequired(false);
        
        builder.HasOne(x => x.Room)
            .WithMany(x => x.Lockers)
            .HasForeignKey("RoomId")
            .IsRequired();

        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.NumberOfFolders)
            .IsRequired();
        
        builder.Property(x => x.IsAvailable)
            .IsRequired();
    }
}