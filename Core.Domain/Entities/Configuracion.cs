using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Representa una entrada de configuración clave-valor persistida en el sistema (ej. PIN de admin).
/// </summary>
public class Configuracion
{
    public string Clave { get; private set; } = string.Empty;
    public string Valor { get; private set; } = string.Empty;

    // Constructor privado para EF Core
    private Configuracion() { }

    public Configuracion(string clave, string valor)
    {
        if (string.IsNullOrWhiteSpace(clave))
        {
            throw new DomainValidationException(nameof(Clave), "La clave de configuración no puede estar vacía.");
        }

        Clave = clave.Trim();
        Valor = valor ?? string.Empty;
    }

    public void ActualizarValor(string nuevoValor)
    {
        Valor = nuevoValor ?? string.Empty;
    }
}
