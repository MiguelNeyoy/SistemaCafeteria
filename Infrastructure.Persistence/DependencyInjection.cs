using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Infrastructure.Persistence.Data;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services)
    {
        // Ubicación externa a la carpeta de instalación (protegida contra actualizaciones de Velopack)
        var dataDirectory = @"C:\UnaMordidaMas\Data";
        try
        {
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }
        catch
        {
            dataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "UnaMordidaMas", "Data");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }

        var dbPath = Path.Combine(dataDirectory, "unamordidamas.db");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IExtraRepository, ExtraRepository>();
        services.AddScoped<ICategoriaExtraRepository, CategoriaExtraRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
            
        return services;
    }
}