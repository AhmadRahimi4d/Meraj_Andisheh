using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("OtpCodes");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(20);

        // هش HMAC-SHA256 به‌صورت هگز: ۶۴ نویسه
        builder.Property(o => o.CodeHash).IsRequired().HasMaxLength(128);

        // جستجوی کدهای یک شماره (محدودیت ساعتی و کد فعال)
        builder.HasIndex(o => new { o.PhoneNumber, o.CreatedAt });
    }
}
