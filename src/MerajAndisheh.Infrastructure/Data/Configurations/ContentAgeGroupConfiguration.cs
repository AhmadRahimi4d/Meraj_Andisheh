using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class ContentAgeGroupConfiguration : IEntityTypeConfiguration<ContentAgeGroup>
{
    public void Configure(EntityTypeBuilder<ContentAgeGroup> builder)
    {
        builder.ToTable("ContentAgeGroups");
        builder.HasKey(x => new { x.ContentId, x.AgeGroupId });

        builder.HasOne(x => x.Content)
            .WithMany(c => c.AgeGroups)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AgeGroup)
            .WithMany()
            .HasForeignKey(x => x.AgeGroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
