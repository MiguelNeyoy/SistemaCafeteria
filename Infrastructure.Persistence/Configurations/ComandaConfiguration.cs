using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ComandaConfiguration : IEntityTypeConfiguration<Comanda>
{
    public void Configure(EntityTypeBuilder<Comanda> builder)
    {
        builder.ToTable("Comandas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.VentaId)
            .IsRequired();

        builder.Property(c => c.IdentificadorCliente)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("General");

        builder.Property(c => c.FechaCreacion)
            .IsRequired();

        builder.Property(c => c.Estado)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasOne<Venta>()
            .WithMany()
            .HasForeignKey(c => c.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.ComandaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
