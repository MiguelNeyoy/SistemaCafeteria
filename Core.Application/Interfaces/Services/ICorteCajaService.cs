using Core.Application.Dtos.Reportes;

namespace Core.Application.Interfaces.Services;

public interface ICorteCajaService
{
    Task<CorteCajaDto> GenerarCorteDiarioAsync(DateTime fecha, decimal fondoInicial);
    Task<CorteCajaDto> GenerarCorteMensualAsync(int anio, int mes);
}
