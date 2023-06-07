using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.DocumentType)
            .HasMaxLength(64)
            .IsRequired();
        
        builder.HasOne(x => x.Folder)
            .WithMany(x => x.Documents)
            .HasForeignKey("FolderId")
            .IsRequired(false);

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey("DepartmentId")
            .IsRequired(false);

        builder.HasOne(x => x.Importer)
            .WithMany()
            .HasForeignKey("ImporterId")
            .IsRequired(false);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.Entry)
            .WithOne()
            .HasForeignKey<Document>(x => x.EntryId)
            .IsRequired(false);
    }
}