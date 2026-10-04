using System;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace SistemaCafeteria.UnitTests.Domain;

public class DescuentoTests
{
    [Fact]
    public void AplicarDescuento_MontoValido_ReduceTotalCorrectamente()
    {
        // Arrange
        var venta = new Venta("Cliente Descuento");
        venta.AgregarItem(1, "Café Americano", 50m, 2); // Subtotal = $100

        // Act
        venta.AplicarDescuento(20m);

        // Assert
        venta.Subtotal.Should().Be(100m);
        venta.Descuento.Should().Be(20m);
        venta.Total.Should().Be(80m);
    }

    [Fact]
    public void AplicarDescuento_Cortesia100Porciento_TotalEsCero()
    {
        // Arrange
        var venta = new Venta("Cortesía");
        venta.AgregarItem(1, "Sandwich", 120m, 1); // Subtotal = $120

        // Act
        venta.AplicarDescuento(120m);

        // Assert
        venta.Subtotal.Should().Be(120m);
        venta.Descuento.Should().Be(120m);
        venta.Total.Should().Be(0m);
    }

    [Fact]
    public void AplicarDescuento_MontoNegativo_LanzaDomainValidationException()
    {
        // Arrange
        var venta = new Venta();
        venta.AgregarItem(1, "Café", 45m, 1);

        // Act
        Action accion = () => venta.AplicarDescuento(-10m);

        // Assert
        accion.Should().Throw<DomainValidationException>()
              .WithMessage("*descuento no puede ser negativo*");
    }

    [Fact]
    public void AplicarDescuento_MontoMayorAlSubtotal_LanzaDomainValidationException()
    {
        // Arrange
        var venta = new Venta();
        venta.AgregarItem(1, "Latte", 60m, 1); // Subtotal = $60

        // Act
        Action accion = () => venta.AplicarDescuento(70m);

        // Assert
        accion.Should().Throw<DomainValidationException>()
              .WithMessage("*no puede ser mayor al subtotal*");
    }

    [Fact]
    public void AplicarDescuento_CuentaYaCobrada_LanzaDomainException()
    {
        // Arrange
        var venta = new Venta();
        venta.AgregarItem(1, "Pastel", 80m, 1);
        venta.Cobrar(TipoDePago.Efectivo, 100m);

        // Act
        Action accion = () => venta.AplicarDescuento(10m);

        // Assert
        accion.Should().Throw<DomainException>()
              .WithMessage("*No se puede modificar el descuento de una cuenta cerrada*");
    }

    [Fact]
    public void Cobrar_ConDescuento_CalculaCambioEnEfectivoCorrectamente()
    {
        // Arrange
        var venta = new Venta("Cliente Efectivo");
        venta.AgregarItem(1, "Desayuno", 150m, 1);
        venta.AplicarDescuento(30m); // Total = $120

        // Act
        venta.Cobrar(TipoDePago.Efectivo, 200m); // Paga con billete de $200

        // Assert
        venta.Total.Should().Be(120m);
        venta.MontoRecibido.Should().Be(200m);
        venta.Cambio.Should().Be(80m);
        venta.Estado.Should().Be(EstadoVenta.Pagado);
    }
}
