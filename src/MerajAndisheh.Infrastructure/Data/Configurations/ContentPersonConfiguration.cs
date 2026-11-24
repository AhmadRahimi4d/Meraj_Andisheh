using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class ContentPersonConfiguration : IEntityTypeConfiguration<ContentPerson>
{
    public void Configure(EntityTypeBuilder<ContentPerson> builder)
    {
        builder.ToTable("ContentPeople");
        builder.HasKey(x => new { x.ContentId, x.PersonId });

        builder.HasOne(x => x.Content)
            .WithMany(c => c.People)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
