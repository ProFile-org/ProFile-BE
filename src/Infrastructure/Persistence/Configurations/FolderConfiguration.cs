using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.HasOne(x => x.Locker)
            .WithMany(x => x.Folders)
            .HasForeignKey("LockerId")
            .IsRequired();

        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.NumberOfDocuments)
            .IsRequired();
        
        builder.Property(x => x.IsAvailable)
            .IsRequired();
    }
}