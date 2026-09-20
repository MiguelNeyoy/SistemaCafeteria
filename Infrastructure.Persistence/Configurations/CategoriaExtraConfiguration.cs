using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CategoriaExtraConfiguration : IEntityTypeConfiguration<CategoriaExtra>
{
    public void Configure(EntityTypeBuilder<CategoriaExtra> builder)
    {
        builder.ToTable("CategoriaExtras");

        // Clave primaria compuesta (CategoriaId, ExtraId)
        builder.HasKey(ce => new { ce.CategoriaId, ce.ExtraId });

        // Relación con Categoria (eliminación en cascada)
        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(ce => ce.CategoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación con Extra (eliminación en cascada)
        builder.HasOne<Extra>()
            .WithMany()
            .HasForeignKey(ce => ce.ExtraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
