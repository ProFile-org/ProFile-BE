using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ImportRequestConfiguration : IEntityTypeConfiguration<ImportRequest>
{
    public void Configure(EntityTypeBuilder<ImportRequest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(x => x.Document)
            .WithOne()
            .HasForeignKey<ImportRequest>(x => x.DocumentId)
            .IsRequired();

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .IsRequired();

        builder.Property(x => x.ImportReason)
            .IsRequired();

        builder.Property(x => x.Status)
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