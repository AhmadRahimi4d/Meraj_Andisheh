using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class ContentTopicConfiguration : IEntityTypeConfiguration<ContentTopic>
{
    public void Configure(EntityTypeBuilder<ContentTopic> builder)
    {
        builder.ToTable("ContentTopics");
        builder.HasKey(x => new { x.ContentId, x.TopicId });

        builder.HasOne(x => x.Content)
            .WithMany(c => c.Topics)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        // حذف موضوعی که محتوا دارد رد می‌شود (قانون ۶.۷)
        builder.HasOne(x => x.Topic)
            .WithMany()
            .HasForeignKey(x => x.TopicId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
