using MerajAndisheh.Core.Domain;
using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.ToTable("ContentItems");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(ContentLimits.TitleMaxLength);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(ContentLimits.SlugMaxLength);
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.Summary).IsRequired().HasMaxLength(ContentLimits.SummaryMaxLength);

        // Body و SearchText بدون سقف طول ← nvarchar(max)
        builder.Property(c => c.SearchText).IsRequired();

        // enumها به‌صورت tinyint
        builder.Property(c => c.Format).HasConversion<byte>();
        builder.Property(c => c.Status).HasConversion<byte>();

        builder.Property(c => c.ThumbnailKey).HasMaxLength(500);
        builder.Property(c => c.LanguageCode).IsRequired().HasMaxLength(2).IsFixedLength();

        // برای فهرست‌های عمومی (منتشرشده‌ها، جدیدترین اول)
        builder.HasIndex(c => new { c.Status, c.PublishedAt });

        builder.HasOne(c => c.ImpactLevel)
            .WithMany()
            .HasForeignKey(c => c.ImpactLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        // CreatedById: کلید خارجی در مرحله‌ی ۴ (جدول کاربران) اضافه می‌شود.
    }
}
