using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VentaItemExtraConfiguration : IEntityTypeConfiguration<VentaItemExtra>
{
    public void Configure(EntityTypeBuilder<VentaItemExtra> builder)
    {
        builder.ToTable("VentaItemExtras");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.VentaItemId)
            .IsRequired();

        builder.Property(e => e.ExtraId)
            .IsRequired();

        builder.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Precio)
            .HasPrecision(10, 2)
            .IsRequired();
    }
}
