using Domain.Entities;
using Domain.Entities.Digital;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder.HasMany(x => x.Users)
            .WithMany(x => x.UserGroups).UsingEntity(
                "Memberships",
                l => l.HasOne(typeof(User)).WithMany().HasForeignKey("UserId").HasPrincipalKey(nameof(User.Id)),
                r => r.HasOne(typeof(UserGroup)).WithMany().HasForeignKey("UserGroupId").HasPrincipalKey(nameof(UserGroup.Id)),
                j => j.HasKey("UserId", "UserGroupId"));
        
        builder.HasAlternateKey(x => x.Name);
    }
}