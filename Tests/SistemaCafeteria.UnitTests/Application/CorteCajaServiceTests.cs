using Core.Application.Interfaces.Repositories;
using Core.Application.UseCases;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace SistemaCafeteria.UnitTests.Application;

public class CorteCajaServiceTests
{
    private readonly Mock<IVentaRepository> _ventaRepositoryMock;
    private readonly CorteCajaService _service;

    public CorteCajaServiceTests()
    {
        _ventaRepositoryMock = new Mock<IVentaRepository>();
        _service = new CorteCajaService(_ventaRepositoryMock.Object);
    }

    [Fact]
    public async Task GenerarCorteDiario_CalculaDesgloseExactoYDineroEnCajon()
    {
        // Arrange
        var hoy = DateTime.Today;

        // Venta 1: Pagada en Efectivo ($100)
        var v1 = new Venta("Cliente 1");
        v1.AgregarItem(1, "Café", 100m, 1);
        v1.Cobrar(TipoDePago.Efectivo, 100m);

        // Venta 2: Pagada con Tarjeta ($150)
        var v2 = new Venta("Cliente 2");
        v2.AgregarItem(2, "Frappé", 150m, 1);
        v2.Cobrar(TipoDePago.Tarjeta);

        // Venta 3: Pagada con Transferencia ($200)
        var v3 = new Venta("Cliente 3");
        v3.AgregarItem(3, "Pastel", 200m, 1);
        v3.Cobrar(TipoDePago.Transferencia);

        // Venta 4: Pagada con Efectivo ($50) pero posteriormente Devuelta
        var v4 = new Venta("Cliente 4");
        v4.AgregarItem(4, "Galleta", 50m, 1);
        v4.Cobrar(TipoDePago.Efectivo, 50m);
        v4.Devolver();

        // Venta 5: Cancelada antes de pagar ($80)
        var v5 = new Venta("Cliente 5");
        v5.AgregarItem(5, "Sandwich", 80m, 1);
        v5.Cancelar();

        // Venta 6: Pendiente / Cuenta Abierta ($60)
        var v6 = new Venta("Cliente 6");
        v6.AgregarItem(6, "Jugo", 60m, 1);

        var ventasDelDia = new List<Venta> { v1, v2, v3, v4, v5, v6 };

        _ventaRepositoryMock.Setup(r => r.ObtenerPorFechaAsync(hoy))
            .ReturnsAsync(ventasDelDia);

        decimal fondoInicial = 500.00m;

        // Act
        var corte = await _service.GenerarCorteDiarioAsync(hoy, fondoInicial);

        // Assert
        corte.Should().NotBeNull();
        corte.FondoInicial.Should().Be(500.00m);

        // Desglose de ventas pagadas: $100 + $150 + $200 = $450
        corte.TotalEfectivo.Should().Be(100.00m);
        corte.TotalTarjeta.Should().Be(150.00m);
        corte.TotalTransferencia.Should().Be(200.00m);
        corte.TotalVentas.Should().Be(450.00m);

        // Devoluciones y cancelaciones
        corte.TotalDevoluciones.Should().Be(50.00m);
        corte.CantidadVentas.Should().Be(3); // Solo pagadas
        corte.CantidadCanceladas.Should().Be(1);
        corte.CantidadDevueltas.Should().Be(1);

        // Dinero esperado en el cajón físico:
        // FondoInicial ($500) + TotalEfectivo ($100) - TotalDevoluciones ($50) = $550.00
        corte.EsperadoEnCajon.Should().Be(550.00m);
    }

    [Fact]
    public async Task GenerarCorteDiario_SinVentas_RetornaTotalesEnCeroYFondoInicial()
    {
        // Arrange
        var hoy = DateTime.Today;
        _ventaRepositoryMock.Setup(r => r.ObtenerPorFechaAsync(hoy))
            .ReturnsAsync(new List<Venta>());

        decimal fondoInicial = 300.00m;

        // Act
        var corte = await _service.GenerarCorteDiarioAsync(hoy, fondoInicial);

        // Assert
        corte.TotalVentas.Should().Be(0m);
        corte.TotalEfectivo.Should().Be(0m);
        corte.TotalTarjeta.Should().Be(0m);
        corte.TotalTransferencia.Should().Be(0m);
        corte.CantidadVentas.Should().Be(0);
        corte.EsperadoEnCajon.Should().Be(300.00m);
    }
}
