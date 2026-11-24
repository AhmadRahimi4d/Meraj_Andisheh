using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class AgeGroupConfiguration : IEntityTypeConfiguration<AgeGroup>
{
    public void Configure(EntityTypeBuilder<AgeGroup> builder)
    {
        builder.ToTable("AgeGroups");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Key).IsRequired().HasMaxLength(20);
        builder.HasIndex(a => a.Key).IsUnique();
        builder.Property(a => a.Name).IsRequired().HasMaxLength(60);
        builder.Property(a => a.ThemeKey).IsRequired().HasMaxLength(20);
    }
}
