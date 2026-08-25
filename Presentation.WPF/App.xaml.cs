using Core.Application;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Presentation.WPF.ViewModels;
using System.Windows;

namespace Presentation.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddApplication();
        services.AddPersistence();

        services.AddTransient<MainWindow>();
        services.AddTransient<MainViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

        mainWindow.Show();
    }
}