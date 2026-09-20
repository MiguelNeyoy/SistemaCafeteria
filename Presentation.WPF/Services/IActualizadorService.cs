using System;
using System.Threading.Tasks;

namespace Presentation.WPF.Services;

/// <summary>
/// Contrato para el control de versiones y actualizaciones automáticas con Velopack.
/// </summary>
public interface IActualizadorService
{
    bool EstaInstalado { get; }
    string VersionActual { get; }
    Task<bool> HayActualizacionDisponibleAsync();
    Task<string?> ObtenerNuevaVersionAsync();
    Task DescargarActualizacionAsync(Action<int>? reportarProgreso = null);
    void AplicarActualizacionYReiniciar();
}
