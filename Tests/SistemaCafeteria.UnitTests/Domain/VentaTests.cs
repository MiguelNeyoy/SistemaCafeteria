using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace SistemaCafeteria.UnitTests.Domain;

public class VentaTests
{
    [Fact]
    public void NuevaVenta_DebeTenerEstadoPendienteYTotalesEnCero()
    {
        // Act
        var venta = new Venta("Mesa 5");

        // Assert
        venta.Estado.Should().Be(EstadoVenta.Pendiente);
        venta.IdentificadorCliente.Should().Be("Mesa 5");
        venta.Subtotal.Should().Be(0m);
        venta.Total.Should().Be(0m);
        venta.Descuento.Should().Be(0m);
        venta.Items.Should().BeEmpty();
    }

    [Fact]
    public void AgregarItem_SinExtras_CalculaSubtotalYTotalCorrectamente()
    {
        // Arrange
        var venta = new Venta("General");

        // Act
        venta.AgregarItem(productoId: 10, productoNombre: "Capuchino", precioUnitario: 45.00m, cantidad: 2);

        // Assert
        venta.Items.Should().HaveCount(1);
        venta.Subtotal.Should().Be(90.00m);
        venta.Total.Should().Be(90.00m);
        venta.Items.First().Subtotal.Should().Be(90.00m);
    }

    [Fact]
    public void AgregarItem_ConExtras_CongelaPreciosYSumaAlTotal()
    {
        // Arrange
        var venta = new Venta("Mesa 1");
        var extras = new List<VentaItemExtra>
        {
            new VentaItemExtra(1, "Leche Deslactosada", 8.00m),
            new VentaItemExtra(2, "Jarabe Vainilla", 10.00m)
        };

        // Act
        // Precio unitario: $50 + $18 de extras = $68 cada uno. Cantidad: 2 => Subtotal: $136
        venta.AgregarItem(1, "Frappé Moka", 50.00m, cantidad: 2, notas: "Poco dulce", extras: extras);

        // Assert
        venta.Items.Should().HaveCount(1);
        venta.Subtotal.Should().Be(136.00m);
        venta.Total.Should().Be(136.00m);
        var item = venta.Items.First();
        item.Extras.Should().HaveCount(2);
        item.PrecioUnitario.Should().Be(50.00m);
        item.Subtotal.Should().Be(136.00m);
    }

    [Fact]
    public void AplicarDescuento_MontoValido_ReduceElTotal()
    {
        // Arrange
        var venta = new Venta("Mesa 2");
        venta.AgregarItem(1, "Pastel", 100.00m, cantidad: 1);

        // Act
        venta.AplicarDescuento(25.00m);

        // Assert
        venta.Subtotal.Should().Be(100.00m);
        venta.Descuento.Should().Be(25.00m);
        venta.Total.Should().Be(75.00m);
    }

    [Fact]
    public void AplicarDescuento_MayorAlSubtotal_LanzaDomainValidationException()
    {
        // Arrange
        var venta = new Venta();
        venta.AgregarItem(1, "Café", 40.00m, cantidad: 1);

        // Act & Assert
        var act = () => venta.AplicarDescuento(50.00m);
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*no puede ser mayor al subtotal*");
    }

    [Fact]
    public void Cobrar_ConEfectivoSuficiente_CambiaEstadoAPagadoYCalculaCambio()
    {
        // Arrange
        var venta = new Venta("Cliente A");
        venta.AgregarItem(1, "Sandwich", 85.00m, cantidad: 1); // Total: $85.00

        // Act
        venta.Cobrar(TipoDePago.Efectivo, montoRecibido: 100.00m);

        // Assert
        venta.Estado.Should().Be(EstadoVenta.Pagado);
        venta.TipoDePago.Should().Be(TipoDePago.Efectivo);
        venta.MontoRecibido.Should().Be(100.00m);
        venta.Cambio.Should().Be(15.00m);
        venta.FechaCierre.Should().NotBeNull();
    }

    [Fact]
    public void Cobrar_ConEfectivoInsuficiente_LanzaDomainValidationException()
    {
        // Arrange
        var venta = new Venta("Cliente B");
        venta.AgregarItem(1, "Sandwich", 85.00m, cantidad: 1);

        // Act & Assert
        var act = () => venta.Cobrar(TipoDePago.Efectivo, montoRecibido: 50.00m);
        act.Should().Throw<DomainValidationException>()
            .WithMessage("*insuficiente*");
    }

    [Fact]
    public void Cobrar_ConTarjeta_NoRequiereMontoRecibidoNiGeneraCambio()
    {
        // Arrange
        var venta = new Venta();
        venta.AgregarItem(1, "Frappé", 60.00m, cantidad: 1);

        // Act
        venta.Cobrar(TipoDePago.Tarjeta);

        // Assert
        venta.Estado.Should().Be(EstadoVenta.Pagado);
        venta.TipoDePago.Should().Be(TipoDePago.Tarjeta);
        venta.MontoRecibido.Should().BeNull();
        venta.Cambio.Should().BeNull();
    }

    [Fact]
    public void Cobrar_VentaSinItems_LanzaDomainException()
    {
        // Arrange
        var venta = new Venta(); // Total: $0, sin ítems

        // Act & Assert
        var act = () => venta.Cobrar(TipoDePago.Efectivo, 100.00m);
        act.Should().Throw<DomainException>()
            .WithMessage("*sin productos*");
    }

    [Fact]
    public void AgregarItem_CuandoVentaYaEstaPagada_LanzaDomainException()
    {
        // Arrange
        var venta = new Venta();
        venta.AgregarItem(1, "Té Chai", 40.00m, 1);
        venta.Cobrar(TipoDePago.Efectivo, 50.00m);

        // Act & Assert: Intentar alterar una venta ya cerrada
        var act = () => venta.AgregarItem(2, "Galleta", 15.00m, 1);
        act.Should().Throw<DomainException>()
            .WithMessage("*cuenta cerrada o cancelada*");
    }

    [Fact]
    public void Cancelar_VentaPendiente_CambiaEstadoACancelado()
    {
        // Arrange
        var venta = new Venta("Mesa 3");
        venta.AgregarItem(1, "Espresso", 30.00m, 1);

        // Act
        venta.Cancelar();

        // Assert
        venta.Estado.Should().Be(EstadoVenta.Cancelado);
        venta.FechaCierre.Should().NotBeNull();
    }

    [Fact]
    public void Devolver_VentaPagada_CambiaEstadoADevuelto()
    {
        // Arrange
        var venta = new Venta();
        venta.AgregarItem(1, "Postre", 55.00m, 1);
        venta.Cobrar(TipoDePago.Tarjeta);

        // Act
        venta.Devolver();

        // Assert
        venta.Estado.Should().Be(EstadoVenta.Devuelto);
    }
}
