using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ComandaItemConfiguration : IEntityTypeConfiguration<ComandaItem>
{
    public void Configure(EntityTypeBuilder<ComandaItem> builder)
    {
        builder.ToTable("ComandaItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ComandaId)
            .IsRequired();

        builder.Property(i => i.ProductoId)
            .IsRequired();

        builder.Property(i => i.ProductoNombre)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Cantidad)
            .IsRequired();

        builder.Property(i => i.NotasCocina)
            .IsRequired(false)
            .HasMaxLength(300);

        builder.HasMany(i => i.Extras)
            .WithOne()
            .HasForeignKey(e => e.ComandaItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Extras)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
