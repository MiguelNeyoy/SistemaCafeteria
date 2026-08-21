using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;

namespace Core.Application.UseCases;

public class PurgaService : IPurgaService
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PurgaService(IVentaRepository ventaRepository, IUnitOfWork unitOfWork)
    {
        _ventaRepository = ventaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> PurgarVentasAntiguasAsync(int diasAntiguedad = 30)
    {
        var fechaLimite = DateTime.Now.Date.AddDays(-diasAntiguedad);
        
        var ventasAntiguas = await _ventaRepository.ObtenerPorRangoFechasAsync(DateTime.MinValue, fechaLimite);
        var totalEliminadas = ventasAntiguas.Count;

        if (totalEliminadas > 0)
        {
            await _ventaRepository.EliminarVentasAnterioresAAsync(fechaLimite);
            await _unitOfWork.SaveChangesAsync();
        }

        return totalEliminadas;
    }
}
