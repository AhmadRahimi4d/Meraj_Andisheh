using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(150);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Bio).HasMaxLength(1000);
        builder.Property(p => p.PhotoKey).HasMaxLength(500);
    }
}
