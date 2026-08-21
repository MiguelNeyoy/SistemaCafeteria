using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ConfiguracionConfiguration : IEntityTypeConfiguration<Configuracion>
{
    public void Configure(EntityTypeBuilder<Configuracion> builder)
    {
        builder.ToTable("Configuraciones");

        builder.HasKey(c => c.Clave);

        builder.Property(c => c.Clave)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Valor)
            .IsRequired()
            .HasMaxLength(500);
    }
}
