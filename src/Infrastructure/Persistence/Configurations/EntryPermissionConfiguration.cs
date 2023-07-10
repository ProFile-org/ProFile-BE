using Domain.Entities.Digital;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class EntryPermissionConfiguration : IEntityTypeConfiguration<EntryPermission>
{
    public void Configure(EntityTypeBuilder<EntryPermission> builder)
    {
        builder.HasKey(x => new { x.EntryId, x.EmployeeId });

        builder.Property(x => x.AllowedOperations)
            .IsRequired();

        builder.Property(x => x.IsSharedRoot)
            .IsRequired();
    }
}