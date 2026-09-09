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
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=pos.db"));

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IExtraRepository, ExtraRepository>();
<<<<<<< HEAD
        services.AddScoped<ICategoriaExtraRepository, CategoriaExtraRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
=======
        services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
>>>>>>> Vistas

        services.AddScoped<IUnitOfWork, UnitOfWork>();
            
        return services;
    }
}