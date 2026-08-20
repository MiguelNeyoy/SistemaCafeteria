using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Precio)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(p => p.CategoriaId)
            .IsRequired();

        builder.Property(p => p.Activo)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
