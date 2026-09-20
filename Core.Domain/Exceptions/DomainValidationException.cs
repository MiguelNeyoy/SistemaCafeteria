namespace Core.Domain.Exceptions;

/// <summary>
/// Excepción lanzada cuando una entidad falla la validación de sus campos o invariantes de negocio.
/// </summary>
public class DomainValidationException : DomainException
{
    public string Campo { get; }

    public DomainValidationException(string campo, string message) : base(message)
    {
        Campo = campo;
    }
}
