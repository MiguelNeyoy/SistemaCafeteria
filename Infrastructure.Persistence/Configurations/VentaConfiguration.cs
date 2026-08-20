using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.IdentificadorCliente)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("General");

        builder.Property(v => v.FechaCreacion)
            .IsRequired();

        builder.Property(v => v.FechaCierre)
            .IsRequired(false);

        builder.Property(v => v.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(v => v.TipoDePago)
            .IsRequired(false)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(v => v.Descuento)
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(v => v.MontoRecibido)
            .HasPrecision(10, 2)
            .IsRequired(false);

        // Propiedades calculadas en memoria
        builder.Ignore(v => v.Subtotal);
        builder.Ignore(v => v.Total);
        builder.Ignore(v => v.Cambio);

        // Colección de ítems con backing field
        builder.HasMany(v => v.Items)
            .WithOne()
            .HasForeignKey(i => i.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(v => v.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
