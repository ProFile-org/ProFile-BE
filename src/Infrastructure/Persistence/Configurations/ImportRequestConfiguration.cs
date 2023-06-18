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
            .WithMany()
            .HasForeignKey("DocumentId")
            .IsRequired();

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey("RoomId")
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();
    }
}