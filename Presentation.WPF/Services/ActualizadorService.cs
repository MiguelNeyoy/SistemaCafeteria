using System;
using System.Reflection;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;

namespace Presentation.WPF.Services;

/// <summary>
/// Implementación de actualizaciones automáticas utilizando Velopack.
/// Soporta modo desarrollo sin fallos y conexión con GitHub Releases.
/// </summary>
public class ActualizadorService : IActualizadorService
{
    private readonly UpdateManager _updateManager;
    private UpdateInfo? _updatePendiente;

    public bool EstaInstalado => _updateManager.IsInstalled;

    public string VersionActual => _updateManager.CurrentVersion?.ToString()
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
        ?? "1.0.0";

    public ActualizadorService()
    {
        // Fuente por defecto en GitHub Releases del repositorio
        var source = new GithubSource("https://github.com/MiguelNeyoy/SistemaCafeteria", null, false);
        _updateManager = new UpdateManager(source);
    }

    public async Task<bool> HayActualizacionDisponibleAsync()
    {
        if (!EstaInstalado) return false;

        try
        {
            _updatePendiente = await _updateManager.CheckForUpdatesAsync();
            return _updatePendiente != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> ObtenerNuevaVersionAsync()
    {
        if (!EstaInstalado) return null;

        try
        {
            _updatePendiente ??= await _updateManager.CheckForUpdatesAsync();
            return _updatePendiente?.TargetFullRelease?.Version?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public async Task DescargarActualizacionAsync(Action<int>? reportarProgreso = null)
    {
        if (!EstaInstalado || _updatePendiente == null) return;

        await _updateManager.DownloadUpdatesAsync(_updatePendiente, reportarProgreso);
    }

    public void AplicarActualizacionYReiniciar()
    {
        if (!EstaInstalado || _updatePendiente == null) return;

        _updateManager.ApplyUpdatesAndRestart(_updatePendiente);
    }
}
