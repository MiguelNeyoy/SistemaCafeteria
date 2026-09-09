using Core.Application.Interfaces.Services;
using Core.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ICategoriaService, CategoriaService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IExtraService, ExtraService>();
<<<<<<< HEAD
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<IComandaService, ComandaService>();
        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ICorteCajaService, CorteCajaService>();
=======
>>>>>>> Vistas
        services.AddScoped<ISeguridadService, SeguridadService>();
        services.AddScoped<IPurgaService, PurgaService>();

        return services;
    }
}