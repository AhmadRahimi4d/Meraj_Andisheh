using MerajAndisheh.Core.Domain.Entities;
using MerajAndisheh.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class StaffPermissionConfiguration : IEntityTypeConfiguration<StaffPermission>
{
    public void Configure(EntityTypeBuilder<StaffPermission> builder)
    {
        builder.ToTable("StaffPermissions");
        builder.HasKey(p => new { p.UserId, p.PermissionKey });

        builder.Property(p => p.PermissionKey).IsRequired().HasMaxLength(60);

        // با حذف کاربر، دسترسی‌هایش هم می‌رود (حذف واقعی همکار فقط از راه دیتابیس ممکن است؛ در پنل فقط غیرفعال‌سازی هست)
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
