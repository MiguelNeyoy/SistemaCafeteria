using Core.Application.Interfaces;
using Infrastructure.Hardware.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Hardware;

public static class DependencyInjection
{
    public static IServiceCollection AddHardwareServices(this IServiceCollection services)
    {
        services.AddScoped<IPrinterService, PrinterService>();
        return services;
    }
}
