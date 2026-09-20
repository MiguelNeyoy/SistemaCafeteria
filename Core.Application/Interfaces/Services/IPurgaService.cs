namespace Core.Application.Interfaces.Services;

public interface IPurgaService
{
    Task<int> ContarVentasAntiguasAsync(int diasAntiguedad = 30);
    Task<int> PurgarVentasAntiguasAsync(int diasAntiguedad = 30);
}
