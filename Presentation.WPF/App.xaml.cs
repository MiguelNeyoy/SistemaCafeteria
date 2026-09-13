using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Core.Application;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Presentation.WPF.ViewModels;
using Presentation.WPF.Services;
using System.Windows;

namespace Presentation.WPF;

/// <summary>  
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private IServiceScope? _appScope;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // 1. Módulos de capas inferiores
        services.AddApplication();
        services.AddPersistence();

        services.AddSingleton<IDialogoService, DialogoService>();

        // 2. Vistas y ViewModels de la capa de Presentación
        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<ConfiguracionMenuViewModel>();
        services.AddTransient<CierreDeCajaViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<MenuViewModel>();
        services.AddTransient<CuentasAbiertasViewModel>();
        services.AddTransient<FinalizarCompraViewModel>();

        // 3. Construye el contenedor con validación estricta de árbol de dependencias
        _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        // 4. Obtener la base de datos y aplicar migraciones en un scope temporal
        using (var migrationScope = _serviceProvider.CreateScope())
        {
            var dbContext = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        // 5. Crear el Scope formal para la sesión de la ventana principal y sus servicios Scoped
        _appScope = _serviceProvider.CreateScope();
        var mainWindow = _appScope.ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _appScope?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}