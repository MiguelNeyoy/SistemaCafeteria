using Core.Application.Dtos.Comandas;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace Core.Application.UseCases;

public class ComandaService : IComandaService
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IVentaRepository _ventaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ComandaService(
        IComandaRepository comandaRepository,
        IVentaRepository ventaRepository,
        IProductoRepository productoRepository,
        IUnitOfWork unitOfWork)
    {
        _comandaRepository = comandaRepository;
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ComandaResumenDto> EnviarACocinaAsync(int ventaId, List<EnviarComandaItemDto> items)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(ventaId)
            ?? throw new DomainException($"Venta con Id {ventaId} no encontrada.");

        if (items == null || !items.Any())
        {
            throw new DomainException("No se pueden enviar comandas vacías a cocina.");
        }

        var comandaItems = new List<ComandaItem>();
        foreach (var itemDto in items)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(itemDto.ProductoId)
                ?? throw new DomainException($"Producto con Id {itemDto.ProductoId} no encontrado.");

            var extras = itemDto.ExtraInstrucciones?
                .Select(instruccion => new ComandaItemExtra(instruccion))
                .ToList();

            comandaItems.Add(new ComandaItem(producto.Id, producto.Nombre, itemDto.Cantidad, itemDto.NotasCocina, extras));
        }

        var comanda = new Comanda(venta.Id, venta.IdentificadorCliente, comandaItems);
        await _comandaRepository.AgregarAsync(comanda);
        await _unitOfWork.SaveChangesAsync();

        return MapToResumenDto(comanda);
    }

    public async Task MarcarEntregadaAsync(int comandaId)
    {
        var comanda = await _comandaRepository.ObtenerPorIdAsync(comandaId)
            ?? throw new DomainException($"Comanda con Id {comandaId} no encontrada.");

        comanda.MarcarEntregada();
        _comandaRepository.Actualizar(comanda);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<ComandaResumenDto>> ObtenerPendientesAsync()
    {
        var comandas = await _comandaRepository.ObtenerPendientesAsync();
        return comandas.Select(MapToResumenDto).ToList();
    }

    public async Task<List<ComandaResumenDto>> ObtenerPorVentaAsync(int ventaId)
    {
        var comandas = await _comandaRepository.ObtenerPorVentaIdAsync(ventaId);
        return comandas.Select(MapToResumenDto).ToList();
    }

    private static ComandaResumenDto MapToResumenDto(Comanda comanda)
    {
        return new ComandaResumenDto
        {
            Id = comanda.Id,
            VentaId = comanda.VentaId,
            IdentificadorCliente = comanda.IdentificadorCliente,
            FechaCreacion = comanda.FechaCreacion,
            Estado = comanda.Estado,
            Items = comanda.Items.Select(i => new ComandaItemResumenDto
            {
                Id = i.Id,
                ProductoId = i.ProductoId,
                ProductoNombre = i.ProductoNombre,
                Cantidad = i.Cantidad,
                NotasCocina = i.NotasCocina,
                Extras = i.Extras.Select(e => e.Nombre).ToList()
            }).ToList()
        };
    }
}
