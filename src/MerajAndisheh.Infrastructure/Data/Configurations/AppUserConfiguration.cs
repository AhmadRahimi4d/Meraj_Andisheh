using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(AccountRules.DisplayNameMaxLength);

        // رده‌ی سنی پروفایل؛ حذف رده‌ی دارای کاربر رد می‌شود
        builder.HasOne<AgeGroup>()
            .WithMany()
            .HasForeignKey(u => u.AgeGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
