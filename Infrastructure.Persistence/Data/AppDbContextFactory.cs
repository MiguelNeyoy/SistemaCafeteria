using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence.Data;

/// <summary>
/// Factoría de diseño para permitir la creación y ejecución de migraciones de EF Core.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        
        // Cadena de conexión temporal utilizada únicamente en tiempo de diseño para generar migraciones
        optionsBuilder.UseSqlite("Data Source=design_time.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}
