using MerajAndisheh.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MerajAndisheh.Infrastructure.Data.Configurations;

public class ImpactLevelConfiguration : IEntityTypeConfiguration<ImpactLevel>
{
    public void Configure(EntityTypeBuilder<ImpactLevel> builder)
    {
        builder.ToTable("ImpactLevels");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Name).IsRequired().HasMaxLength(40);
    }
}
