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

        // شماره‌ی موبایل (مرحله‌ی ۵): قالب یکسان 09xxxxxxxxx؛ طول محدود تا بتواند ایندکس شود
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);

        // هر شماره فقط یک حساب. کاربرانِ بدون شماره (ثبت‌نام با ایمیل) از این قید بیرون‌اند.
        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL");

        // رده‌ی سنی پروفایل؛ حذف رده‌ی دارای کاربر رد می‌شود
        builder.HasOne<AgeGroup>()
            .WithMany()
            .HasForeignKey(u => u.AgeGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
