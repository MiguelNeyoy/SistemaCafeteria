namespace Core.Application.Interfaces.Services;

public interface IPurgaService
{
    Task<int> PurgarVentasAntiguasAsync(int diasAntiguedad = 30);
}
