using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class BorrowConfiguration : IEntityTypeConfiguration<Borrow>
{
    public void Configure(EntityTypeBuilder<Borrow> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.HasOne(x => x.Borrower)
            .WithMany()
            .HasForeignKey("BorrowerId")
            .IsRequired();

        builder.HasOne(x => x.Document)
            .WithMany()
            .HasForeignKey("DocumentId")
            .IsRequired();

        builder.Property(x => x.BorrowTime)
            .IsRequired();

        builder.Property(x => x.DueTime)
            .IsRequired();

        builder.Property(x => x.Reason)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();
    }
}