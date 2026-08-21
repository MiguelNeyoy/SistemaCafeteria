using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Entities;
using Core.Domain.Exceptions;

namespace Core.Application.UseCases;

public class VentaService : IVentaService
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IExtraRepository _extraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VentaService(
        IVentaRepository ventaRepository,
        IProductoRepository productoRepository,
        IExtraRepository extraRepository,
        IUnitOfWork unitOfWork)
    {
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _extraRepository = extraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VentaResumenDto> CrearCuentaAsync(string? identificadorCliente = null)
    {
        var venta = new Venta(identificadorCliente);
        await _ventaRepository.AgregarAsync(venta);
        await _unitOfWork.SaveChangesAsync();

        return MapToResumenDto(venta);
    }

    public async Task<VentaResumenDto> AgregarItemAsync(int ventaId, AgregarItemVentaDto dto)
    {
        var venta = await _ventaRepository.ObtenerPorIdConDetallesAsync(ventaId)
            ?? throw new DomainException($"Venta con Id {ventaId} no encontrada.");

        var producto = await _productoRepository.ObtenerPorIdAsync(dto.ProductoId)
            ?? throw new DomainException($"Producto con Id {dto.ProductoId} no encontrado.");

        var extrasSnapshot = new List<VentaItemExtra>();
        if (dto.ExtraIds != null && dto.ExtraIds.Any())
        {
            foreach (var extraId in dto.ExtraIds)
            {
                var extra = await _extraRepository.ObtenerPorIdAsync(extraId)
                    ?? throw new DomainException($"Extra con Id {extraId} no encontrado.");

                extrasSnapshot.Add(new VentaItemExtra(extra.Id, extra.Nombre, extra.Precio));
            }
        }

        // Se pasan los datos congelados del producto y extras en el momento de la orden
        venta.AgregarItem(producto.Id, producto.Nombre, producto.Precio, dto.Cantidad, dto.Notas, extrasSnapshot);

        _ventaRepository.Actualizar(venta);
        await _unitOfWork.SaveChangesAsync();

        return MapToResumenDto(venta);
    }

    public async Task<VentaResumenDto> RemoverItemAsync(int ventaId, int ventaItemId)
    {
        var venta = await _ventaRepository.ObtenerPorIdConDetallesAsync(ventaId)
            ?? throw new DomainException($"Venta con Id {ventaId} no encontrada.");

        var item = venta.Items.FirstOrDefault(i => i.Id == ventaItemId)
            ?? throw new DomainException($"Ítem de venta con Id {ventaItemId} no encontrado en la cuenta.");

        venta.RemoverItem(item);

        _ventaRepository.Actualizar(venta);
        await _unitOfWork.SaveChangesAsync();

        return MapToResumenDto(venta);
    }

    public async Task<VentaResumenDto> AplicarDescuentoAsync(int ventaId, decimal montoDescuento)
    {
        var venta = await _ventaRepository.ObtenerPorIdConDetallesAsync(ventaId)
            ?? throw new DomainException($"Venta con Id {ventaId} no encontrada.");

        venta.AplicarDescuento(montoDescuento);

        _ventaRepository.Actualizar(venta);
        await _unitOfWork.SaveChangesAsync();

        return MapToResumenDto(venta);
    }

    public async Task<VentaResumenDto> CobrarAsync(CobrarVentaDto dto)
    {
        var venta = await _ventaRepository.ObtenerPorIdConDetallesAsync(dto.VentaId)
            ?? throw new DomainException($"Venta con Id {dto.VentaId} no encontrada.");

        venta.Cobrar(dto.TipoDePago, dto.MontoRecibido);

        _ventaRepository.Actualizar(venta);
        await _unitOfWork.SaveChangesAsync();

        return MapToResumenDto(venta);
    }

    public async Task<VentaResumenDto> DevolverAsync(int ventaId)
    {
        var venta = await _ventaRepository.ObtenerPorIdConDetallesAsync(ventaId)
            ?? throw new DomainException($"Venta con Id {ventaId} no encontrada.");

        venta.Devolver();

        _ventaRepository.Actualizar(venta);
        await _unitOfWork.SaveChangesAsync();

        return MapToResumenDto(venta);
    }

    public async Task CancelarAsync(int ventaId)
    {
        var venta = await _ventaRepository.ObtenerPorIdConDetallesAsync(ventaId)
            ?? throw new DomainException($"Venta con Id {ventaId} no encontrada.");

        venta.Cancelar();

        _ventaRepository.Actualizar(venta);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<VentaResumenDto?> ObtenerPorIdAsync(int id)
    {
        var venta = await _ventaRepository.ObtenerPorIdConDetallesAsync(id);
        return venta == null ? null : MapToResumenDto(venta);
    }

    public async Task<List<VentaResumenDto>> ObtenerPendientesAsync()
    {
        var ventas = await _ventaRepository.ObtenerPendientesAsync();
        return ventas.Select(MapToResumenDto).ToList();
    }

    public async Task<List<VentaResumenDto>> ObtenerPorFechaAsync(DateTime fecha)
    {
        var ventas = await _ventaRepository.ObtenerPorFechaAsync(fecha);
        return ventas.Select(MapToResumenDto).ToList();
    }

    private static VentaResumenDto MapToResumenDto(Venta venta)
    {
        return new VentaResumenDto
        {
            Id = venta.Id,
            IdentificadorCliente = venta.IdentificadorCliente,
            FechaCreacion = venta.FechaCreacion,
            FechaCierre = venta.FechaCierre,
            Estado = venta.Estado,
            TipoDePago = venta.TipoDePago,
            Subtotal = venta.Subtotal,
            Descuento = venta.Descuento,
            Total = venta.Total,
            MontoRecibido = venta.MontoRecibido,
            Cambio = venta.Cambio,
            Items = venta.Items.Select(i => new VentaItemDto
            {
                Id = i.Id,
                ProductoId = i.ProductoId,
                ProductoNombre = i.ProductoNombre,
                PrecioUnitario = i.PrecioUnitario,
                Cantidad = i.Cantidad,
                Notas = i.Notas,
                Subtotal = i.Subtotal,
                Extras = i.Extras.Select(e => new VentaItemExtraDto
                {
                    Id = e.Id,
                    ExtraId = e.ExtraId,
                    Nombre = e.Nombre,
                    Precio = e.Precio
                }).ToList()
            }).ToList()
        };
    }
}
