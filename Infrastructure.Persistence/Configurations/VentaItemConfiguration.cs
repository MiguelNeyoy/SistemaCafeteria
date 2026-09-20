using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VentaItemConfiguration : IEntityTypeConfiguration<VentaItem>
{
    public void Configure(EntityTypeBuilder<VentaItem> builder)
    {
        builder.ToTable("VentaItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.VentaId)
            .IsRequired();

        builder.Property(i => i.ProductoId)
            .IsRequired();

        builder.Property(i => i.ProductoNombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.PrecioUnitario)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(i => i.Cantidad)
            .IsRequired();

        builder.Property(i => i.Notas)
            .IsRequired(false)
            .HasMaxLength(300);

        // Propiedades calculadas en memoria
        builder.Ignore(i => i.Subtotal);
        builder.Ignore(i => i.SubtotalExtras);

        // Colección de extras con backing field
        builder.HasMany(i => i.Extras)
            .WithOne()
            .HasForeignKey(e => e.VentaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Extras)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
