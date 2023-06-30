using Domain.Entities.Digital;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Path)
            .IsRequired();

        builder.HasOne(x => x.File)
            .WithOne()
            .HasForeignKey<Entry>(x => x.FileId)
            .IsRequired(false);

        builder.HasOne(x => x.Uploader)
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .IsRequired();
        
        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .IsRequired();
    }
}