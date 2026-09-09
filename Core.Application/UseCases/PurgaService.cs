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

    public async Task<int> ContarVentasAntiguasAsync(int diasAntiguedad = 30)
    {
        var fechaLimite = DateTime.Now.Date.AddDays(-diasAntiguedad);
<<<<<<< HEAD
        return await _ventaRepository.ContarVentasAnterioresAAsync(fechaLimite);
=======
        var ventasAntiguas = await _ventaRepository.ObtenerPorRangoFechasAsync(DateTime.MinValue, fechaLimite);
        return ventasAntiguas.Count;
>>>>>>> Vistas
    }

    public async Task<int> PurgarVentasAntiguasAsync(int diasAntiguedad = 30)
    {
        var fechaLimite = DateTime.Now.Date.AddDays(-diasAntiguedad);
        var totalEliminadas = await _ventaRepository.ContarVentasAnterioresAAsync(fechaLimite);

        if (totalEliminadas > 0)
        {
            await _ventaRepository.EliminarVentasAnterioresAAsync(fechaLimite);
            await _unitOfWork.SaveChangesAsync();
        }

        return totalEliminadas;
    }
}
