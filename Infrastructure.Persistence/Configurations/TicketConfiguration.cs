using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.VentaId)
            .IsRequired();

        builder.Property(t => t.Folio)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.FechaEmision)
            .IsRequired();

        builder.Property(t => t.EsReimpresion)
            .IsRequired()
            .HasDefaultValue(false);

        // Folio único
        builder.HasIndex(t => t.Folio)
            .IsUnique();

        builder.HasOne<Venta>()
            .WithMany()
            .HasForeignKey(t => t.VentaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
