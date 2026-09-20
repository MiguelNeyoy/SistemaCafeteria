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
        var dataDirectory = @"C:\UnaMordida\Data";
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
                "UnaMordida", "Data");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }

        var dbPath = Path.Combine(dataDirectory, "unamordida.db");

        // Migración transparente si existe una base de datos previa con el nombre anterior
        if (!File.Exists(dbPath))
        {
            var legacyPaths = new[]
            {
                @"C:\UnaMordidaMas\Data\unamordidamas.db",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UnaMordidaMas", "Data", "unamordidamas.db")
            };

            foreach (var legacyDb in legacyPaths)
            {
                if (File.Exists(legacyDb))
                {
                    try
                    {
                        File.Copy(legacyDb, dbPath, overwrite: false);
                        if (File.Exists(legacyDb + "-wal")) File.Copy(legacyDb + "-wal", dbPath + "-wal", overwrite: false);
                        if (File.Exists(legacyDb + "-shm")) File.Copy(legacyDb + "-shm", dbPath + "-shm", overwrite: false);
                        break;
                    }
                    catch
                    {
                        // Si falla la copia, se creará una base nueva o se reintentará en el próximo inicio
                    }
                }
            }
        }

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