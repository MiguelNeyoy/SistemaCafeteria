using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ComandaItemExtraConfiguration : IEntityTypeConfiguration<ComandaItemExtra>
{
    public void Configure(EntityTypeBuilder<ComandaItemExtra> builder)
    {
        builder.ToTable("ComandaItemExtras");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ComandaItemId)
            .IsRequired();

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(100);
    }
}
