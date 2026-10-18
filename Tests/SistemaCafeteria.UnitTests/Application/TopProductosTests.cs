using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.UseCases;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace SistemaCafeteria.UnitTests.Application;

public class TopProductosTests
{
    private readonly Mock<IVentaRepository> _ventaRepoMock = new();
    private readonly Mock<IProductoRepository> _productoRepoMock = new();
    private readonly Mock<IExtraRepository> _extraRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly VentaService _sut;

    public TopProductosTests()
    {
        _sut = new VentaService(
            _ventaRepoMock.Object,
            _productoRepoMock.Object,
            _extraRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ObtenerTopProductosMesAsync_PorDefecto_DevuelveHasta15ProductosMasVendidos()
    {
        // Arrange: 20 productos distintos vendidos en cuentas pagadas
        var venta = new Venta("Cliente Top");
        for (int i = 1; i <= 20; i++)
        {
            venta.AgregarItem(i, $"Producto #{i:D2}", 50m, cantidad: 21 - i);
        }
        venta.Cobrar(TipoDePago.Tarjeta);

        _ventaRepoMock.Setup(r => r.ObtenerPorRangoFechasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Venta> { venta });

        // Act: Llamar sin especificar límite (debe usar el nuevo default = 15)
        var top = await _sut.ObtenerTopProductosMesAsync(2026, 10);

        // Assert
        top.Should().HaveCount(15);
        top.First().Posicion.Should().Be(1);
        top.First().ProductoId.Should().Be(1);
        top.First().CantidadVendida.Should().Be(20);

        top.Last().Posicion.Should().Be(15);
        top.Last().ProductoId.Should().Be(15);
        top.Last().CantidadVendida.Should().Be(6);
    }

    [Fact]
    public async Task PruebaDeCaos_MasDe25ProductosCuentasCanceladasDevueltasRenombradosYEmpates_ConsolidaYOrdenaTop15Exacto()
    {
        // Arrange
        var ventasMes = new List<Venta>();

        // 1. Ruido caótico: Cuentas Canceladas, Devueltas y Pendientes con cantidades gigantescas (999 unidades)
        // que JAMÁS deben entrar al Top 15
        var vCancelada = new Venta("Cancelada");
        vCancelada.AgregarItem(901, "Producto Fantasma Cancelado", 100m, 999);
        vCancelada.Cancelar();
        ventasMes.Add(vCancelada);

        var vDevuelta = new Venta("Devuelta");
        vDevuelta.AgregarItem(902, "Producto Fantasma Devuelto", 100m, 999);
        vDevuelta.Cobrar(TipoDePago.Efectivo, 99900m);
        vDevuelta.Devolver();
        ventasMes.Add(vDevuelta);

        var vPendiente = new Venta("Pendiente");
        vPendiente.AgregarItem(903, "Producto Fantasma Pendiente", 100m, 999);
        ventasMes.Add(vPendiente);

        // 2. Producto renombrado/preformateado a mitad de mes (mismo ProductoId = 100):
        // Primera mitad del mes se vendió como "cafe de la olla" (30 uds) y luego como "Café de la Olla" (25 uds) -> Total = 55 uds (#1 del mes)
        var vAntigua = new Venta("Cliente 1");
        vAntigua.AgregarItem(100, "cafe de la olla", 45m, 30);
        vAntigua.Cobrar(TipoDePago.Efectivo, 2000m);
        ventasMes.Add(vAntigua);

        var vReciente = new Venta("Cliente 2");
        vReciente.AgregarItem(100, "Café de la Olla", 45m, 25);
        vReciente.Cobrar(TipoDePago.Tarjeta);
        ventasMes.Add(vReciente);

        // 3. Empate en unidades vendidas (40 uds cada uno), desempatado por TotalRecaudado (incluyendo extras) y luego alfabético
        var vEmpates = new Venta("Cliente Empates");
        vEmpates.AgregarItem(201, "Frappé Moka", 65m, 40); // $2,600
        vEmpates.AgregarItem(202, "Bagel Salmón", 95m, 40); // $3,800 -> Debe ir antes que Frappé Moka
        vEmpates.Cobrar(TipoDePago.Transferencia);
        ventasMes.Add(vEmpates);

        // 4. Rellenar con otros 22 productos pagados (cantidades de 1 a 22) para superar los 25 productos en el mes
        var vResto = new Venta("Cliente Resto");
        for (int i = 1; i <= 22; i++)
        {
            vResto.AgregarItem(i, $"Especialidad #{i:D2}", 50m, cantidad: i);
        }
        vResto.Cobrar(TipoDePago.Tarjeta);
        ventasMes.Add(vResto);

        _ventaRepoMock.Setup(r => r.ObtenerPorRangoFechasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventasMes);

        // Act
        var top15 = await _sut.ObtenerTopProductosMesAsync(2026, 10, 15);

        // Assert
        top15.Should().HaveCount(15);

        // Ningún producto de ventas canceladas, devueltas o pendientes debe aparecer
        top15.Should().NotContain(p => p.ProductoId >= 900);

        // Posición #1: ProductoId 100 consolidado con 55 unidades y su nombre más reciente ("Café de la Olla")
        top15[0].Posicion.Should().Be(1);
        top15[0].ProductoId.Should().Be(100);
        top15[0].Nombre.Should().Be("Café de la Olla");
        top15[0].CantidadVendida.Should().Be(55);

        // Posición #2 y #3: Desempate por monto recaudado entre Bagel Salmón ($3,800) y Frappé Moka ($2,600)
        top15[1].Posicion.Should().Be(2);
        top15[1].ProductoId.Should().Be(202);
        top15[1].CantidadVendida.Should().Be(40);
        top15[1].TotalRecaudado.Should().Be(3800m);

        top15[2].Posicion.Should().Be(3);
        top15[2].ProductoId.Should().Be(201);
        top15[2].CantidadVendida.Should().Be(40);
        top15[2].TotalRecaudado.Should().Be(2600m);

        // Verificar numeración correlativa 1..15 sin huecos
        top15.Select(p => p.Posicion).Should().ContainInOrder(Enumerable.Range(1, 15));
    }
}
