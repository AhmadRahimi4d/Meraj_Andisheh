using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class ContentFileConfiguration : IEntityTypeConfiguration<ContentFile>
{
    public void Configure(EntityTypeBuilder<ContentFile> builder)
    {
        builder.ToTable("ContentFiles");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Kind).HasConversion<byte>();
        builder.Property(f => f.StorageType).HasConversion<byte>();
        builder.Property(f => f.StorageKey).HasMaxLength(500);
        builder.Property(f => f.ExternalUrl).HasMaxLength(1000);
        builder.Property(f => f.OriginalFileName).IsRequired().HasMaxLength(260);
        builder.Property(f => f.MimeType).IsRequired().HasMaxLength(150);

        builder.HasOne<ContentItem>()
            .WithMany(c => c.Files)
            .HasForeignKey(f => f.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
