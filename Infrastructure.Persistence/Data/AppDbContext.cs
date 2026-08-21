using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data;

/// <summary>
/// Contexto de Entity Framework Core para la base de datos SQLite del sistema POS.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Extra> Extras => Set<Extra>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaItem> VentaItems => Set<VentaItem>();
    public DbSet<VentaItemExtra> VentaItemExtras => Set<VentaItemExtra>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<ComandaItem> ComandaItems => Set<ComandaItem>();
    public DbSet<ComandaItemExtra> ComandaItemExtras => Set<ComandaItemExtra>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente todas las configuraciones IEntityTypeConfiguration del ensamblado
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
