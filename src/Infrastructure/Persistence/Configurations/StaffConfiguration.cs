using Domain.Entities.Physical;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("UserId");
        builder.HasOne(x => x.Room)
            .WithOne(x => x.Staff)           
            .HasForeignKey<Staff>(x => x.RoomId);

        builder.HasOne(x => x.User)
            .WithOne(x => x.Staff)
            .HasForeignKey<Staff>(x => x.Id);
    }
}