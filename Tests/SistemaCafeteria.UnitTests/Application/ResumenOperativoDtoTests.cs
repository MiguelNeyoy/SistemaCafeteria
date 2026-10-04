using System;
using System.Collections.Generic;
using Core.Application.Dtos.Reportes;
using Core.Application.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace SistemaCafeteria.UnitTests.Application;

public class ResumenOperativoDtoTests
{
    [Fact]
    public void ResumenOperativoDto_PuedeInicializarseCorrectamente_ConTicketsYTotales()
    {
        // Arrange & Act
        var fecha = new DateTime(2026, 10, 4);
        var resumen = new ResumenOperativoDto
        {
            Fecha = fecha,
            TotalVentas = 500m,
            CantidadVentas = 3,
            TotalEfectivo = 200m,
            TotalTarjeta = 200m,
            TotalTransferencia = 100m,
            TotalDescuentos = 20m,
            TicketPromedio = 166.67m,
            Tickets = new List<TicketResumenOperativoDto>
            {
                new() { Folio = 1, Hora = fecha.AddHours(9), TipoPago = "Efectivo", Total = 100m, CantidadItems = 2 },
                new() { Folio = 2, Hora = fecha.AddHours(10), TipoPago = "Tarjeta", Total = 200m, CantidadItems = 3 },
                new() { Folio = 3, Hora = fecha.AddHours(11), TipoPago = "Transferencia", Total = 200m, CantidadItems = 1 }
            }
        };

        // Assert
        resumen.Fecha.Should().Be(fecha);
        resumen.TotalVentas.Should().Be(500m);
        resumen.CantidadVentas.Should().Be(3);
        resumen.TotalEfectivo.Should().Be(200m);
        resumen.TotalTarjeta.Should().Be(200m);
        resumen.TotalTransferencia.Should().Be(100m);
        resumen.TotalDescuentos.Should().Be(20m);
        resumen.TicketPromedio.Should().Be(166.67m);
        resumen.Tickets.Should().HaveCount(3);
        resumen.Tickets[0].Folio.Should().Be(1);
        resumen.Tickets[0].Cliente.Should().Be("General");
    }

    [Fact]
    public async Task PrinterServiceMock_RecibeResumenOperativoCorrectamente()
    {
        // Arrange
        var printerMock = new Mock<IPrinterService>();
        var resumen = new ResumenOperativoDto
        {
            Fecha = DateTime.Today,
            TotalVentas = 150m,
            CantidadVentas = 1,
            TotalEfectivo = 150m,
            TicketPromedio = 150m,
            Tickets = new List<TicketResumenOperativoDto>
            {
                new() { Folio = 10, Hora = DateTime.Now, Total = 150m, TipoPago = "Efectivo" }
            }
        };

        printerMock
            .Setup(p => p.ImprimirResumenOperativoAsync(It.IsAny<ResumenOperativoDto>()))
            .Returns(Task.CompletedTask);

        // Act
        await printerMock.Object.ImprimirResumenOperativoAsync(resumen);

        // Assert
        printerMock.Verify(p => p.ImprimirResumenOperativoAsync(It.Is<ResumenOperativoDto>(r =>
            r.TotalVentas == 150m &&
            r.Tickets.Count == 1 &&
            r.Tickets[0].Folio == 10
        )), Times.Once);
    }
}
