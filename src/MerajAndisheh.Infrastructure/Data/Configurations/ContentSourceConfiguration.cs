using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class ContentSourceConfiguration : IEntityTypeConfiguration<ContentSource>
{
    public void Configure(EntityTypeBuilder<ContentSource> builder)
    {
        builder.ToTable("ContentSources");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Title).IsRequired().HasMaxLength(300);
        builder.Property(s => s.Url).HasMaxLength(1000);
        builder.Property(s => s.Note).HasMaxLength(500);

        builder.HasOne<ContentItem>()
            .WithMany(c => c.Sources)
            .HasForeignKey(s => s.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
