using ClosedXML.Excel;
using Core.Application.Dtos.Ventas;
using Core.Application.UseCases;
using Core.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace SistemaCafeteria.UnitTests.Application;

public class ExportadorVentasExcelTests
{
    private readonly ExportadorVentasExcelService _exportador = new();

    [Fact]
    public void GenerarExcelVentas_ListaVacia_GeneraArchivoValidoConTotalesEnCero()
    {
        // Act
        byte[] bytes = _exportador.GenerarExcelVentas(
            Array.Empty<VentaResumenDto>(),
            "Hoy - 08/11/2026",
            "Pago: Efectivo");

        // Assert
        bytes.Should().NotBeNullOrEmpty();
        bytes.Take(4).Should().Equal(0x50, 0x4B, 0x03, 0x04); // Firma mágica ZIP/OpenXML

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);

        wb.Worksheets.Should().HaveCount(2);
        var wsVentas = wb.Worksheet("Ventas");
        var wsDetalle = wb.Worksheet("Detalle Artículos");

        wsVentas.Cell("A1").GetString().Should().Contain("UNA MORDIDA");
        wsVentas.Cell("A2").GetString().Should().Contain("Hoy - 08/11/2026");
        wsVentas.Cell("A2").GetString().Should().Contain("Pago: Efectivo");

        // Fila de totales (fila 6 cuando hay 0 datos)
        wsVentas.Cell(6, 1).GetString().Should().Contain("TOTALES");
        wsVentas.Cell(6, 11).GetValue<decimal>().Should().Be(0m);
        wsDetalle.Cell("A1").GetString().Should().Contain("DETALLE POR ARTÍCULO");
    }

    [Fact]
    public void GenerarExcelVentas_ConFiltrosAplicadosYVentasVariadas_ReflejaExactamenteDatosYTotales()
    {
        // Arrange
        var fecha = new DateTime(2026, 11, 8, 10, 30, 0);
        var ventas = new List<VentaResumenDto>
        {
            new()
            {
                Id = 101,
                IdentificadorCliente = "Mesa 4 - Sofía",
                FechaCreacion = fecha,
                FechaCierre = fecha.AddMinutes(15),
                Estado = EstadoVenta.Pagado,
                TipoDePago = TipoDePago.Tarjeta,
                Subtotal = 150m,
                Descuento = 15m,
                Total = 135m,
                Items =
                [
                    new VentaItemDto
                    {
                        ProductoId = 1,
                        ProductoNombre = "Capuchino Vainilla",
                        PrecioUnitario = 65m,
                        Cantidad = 2,
                        Subtotal = 130m,
                        Notas = "Deslactosada",
                        Extras = [new VentaItemExtraDto { Nombre = "Shot Extra", Precio = 20m }]
                    }
                ]
            },
            new()
            {
                Id = 102,
                IdentificadorCliente = "Cancelada Demo",
                FechaCreacion = fecha.AddMinutes(30),
                Estado = EstadoVenta.Cancelado,
                TipoDePago = null,
                Subtotal = 80m,
                Descuento = 0m,
                Total = 80m,
                Items =
                [
                    new VentaItemDto
                    {
                        ProductoId = 2,
                        ProductoNombre = "Panini Clásico",
                        PrecioUnitario = 80m,
                        Cantidad = 1,
                        Subtotal = 80m
                    }
                ]
            }
        };

        // Act
        byte[] bytes = _exportador.GenerarExcelVentas(
            ventas,
            "Del 01/11/2026 al 08/11/2026",
            "Pago: Tarjeta | Búsqueda: \"Sofía\"");

        // Assert
        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);

        var wsVentas = wb.Worksheet("Ventas");
        wsVentas.Cell("A2").GetString().Should().Contain("Pago: Tarjeta | Búsqueda: \"Sofía\"");

        // Fila 5 -> Venta #101
        wsVentas.Cell(5, 1).GetValue<int>().Should().Be(101);
        wsVentas.Cell(5, 4).GetString().Should().Be("Mesa 4 - Sofía");
        wsVentas.Cell(5, 5).GetString().Should().Contain("2x Capuchino Vainilla").And.Contain("+Shot Extra");
        wsVentas.Cell(5, 7).GetString().Should().Be("Tarjeta");
        wsVentas.Cell(5, 8).GetString().Should().Be("Pagada");
        wsVentas.Cell(5, 9).GetValue<decimal>().Should().Be(150m);
        wsVentas.Cell(5, 10).GetValue<decimal>().Should().Be(15m);
        wsVentas.Cell(5, 11).GetValue<decimal>().Should().Be(135m);

        // Fila 6 -> Venta #102 (Cancelada)
        wsVentas.Cell(6, 1).GetValue<int>().Should().Be(102);
        wsVentas.Cell(6, 7).GetString().Should().Be("—");
        wsVentas.Cell(6, 8).GetString().Should().Be("Cancelada");

        // Fila 8 -> Totales con SUMIFS excluyendo canceladas
        wsVentas.Cell(8, 11).FormulaA1.Should().Contain("SUMIFS");
        wsVentas.Cell(8, 11).FormulaA1.Should().Contain("Pagada");

        // Hoja Detalle Artículos (datos desde fila 4)
        var wsDetalle = wb.Worksheet("Detalle Artículos");
        wsDetalle.Cell(4, 1).GetValue<int>().Should().Be(101);
        wsDetalle.Cell(4, 7).GetString().Should().Be("Capuchino Vainilla");
        wsDetalle.Cell(4, 9).GetString().Should().Contain("Shot Extra");
        wsDetalle.Cell(4, 10).GetString().Should().Be("Deslactosada");
        wsDetalle.Cell(5, 1).GetValue<int>().Should().Be(102);
    }

    [Fact]
    public void PruebaDeCaos_500VentasConCaracteresControlInvisiblesFormulasYTamanoExtremo_GeneraExcelSinCorrupcion()
    {
        // Arrange: 500 ventas caóticas en memoria con cadenas hostiles, Unicode invisible, emojis y centavos irregulares
        var fechaBase = new DateTime(2026, 10, 1, 7, 15, 0);
        var ventasCaoticas = new List<VentaResumenDto>(500);
        var clientesHostiles = new[]
        {
            "=CMD|' /C calc'!A0",
            "+1+1\x00\x07\x1B",
            "Mesa \u200B\uFEFF Café & <\"Ñoño\">",
            "@SUM(A1:A10) ☕🍰",
            new string('X', 100)
        };

        var estados = new[] { EstadoVenta.Pagado, EstadoVenta.Pagado, EstadoVenta.Pagado, EstadoVenta.Cancelado, EstadoVenta.Devuelto };
        var metodos = new[] { TipoDePago.Efectivo, TipoDePago.Tarjeta, TipoDePago.Transferencia };

        decimal sumaEsperadaPagadas = 0m;
        int totalLineasProductoEsperadas = 0;

        for (int i = 1; i <= 500; i++)
        {
            var estado = estados[i % estados.Length];
            var metodo = estado == EstadoVenta.Pagado ? metodos[i % metodos.Length] : (TipoDePago?)null;
            int numItems = i % 10 == 0 ? 0 : (i % 5) + 1; // Incluye ventas sin items y multi-items

            var items = new List<VentaItemDto>();
            decimal subtotal = 0m;

            for (int j = 1; j <= numItems; j++)
            {
                decimal precio = Math.Round(35.33m + j * 12.17m, 2);
                int cant = (j % 3) + 1;
                decimal subLinea = Math.Round(precio * cant, 2);
                subtotal += subLinea;

                string notaExtrema = i == 250 && j == 1
                    ? new string('N', 33000) // Excede el límite de 32,767 chars de celda Excel
                    : $"Nota \t\n\x01 {i}-{j} <especial & \"café\">";

                items.Add(new VentaItemDto
                {
                    ProductoId = j,
                    ProductoNombre = $"Producto {j} ☕",
                    PrecioUnitario = precio,
                    Cantidad = cant,
                    Subtotal = subLinea,
                    Notas = notaExtrema,
                    Extras = j % 2 == 0
                        ? [new VentaItemExtraDto { Nombre = "Leche Almendra \u200B", Precio = 15m }]
                        : []
                });
                totalLineasProductoEsperadas++;
            }

            decimal descuento = i % 7 == 0 ? subtotal : Math.Round(subtotal * 0.10m, 2); // Incluye 100% descuento
            decimal total = Math.Max(0m, subtotal - descuento);

            if (estado == EstadoVenta.Pagado)
            {
                sumaEsperadaPagadas += total;
            }

            ventasCaoticas.Add(new VentaResumenDto
            {
                Id = 1000 + i,
                IdentificadorCliente = clientesHostiles[i % clientesHostiles.Length],
                FechaCreacion = fechaBase.AddMinutes(i * 17),
                FechaCierre = estado == EstadoVenta.Pagado ? fechaBase.AddMinutes(i * 17 + 12) : null,
                Estado = estado,
                TipoDePago = metodo,
                Subtotal = subtotal,
                Descuento = descuento,
                Total = total,
                Items = items
            });
        }

        // Act
        byte[] bytes = _exportador.GenerarExcelVentas(
            ventasCaoticas,
            "Prueba Masiva Octubre \x00\u200B",
            "Todos los filtros");

        // Assert
        bytes.Take(4).Should().Equal(0x50, 0x4B, 0x03, 0x04);

        using var ms = new MemoryStream(bytes);
        using var wb = new XLWorkbook(ms);

        var wsVentas = wb.Worksheet("Ventas");
        var wsDetalle = wb.Worksheet("Detalle Artículos");

        // Verificar que las 500 filas están presentes (filas 5..504) y sus totales pagados cuadran al centavo
        decimal sumaEnExcelPagadas = 0m;
        for (int fila = 5; fila < 5 + 500; fila++)
        {
            wsVentas.Cell(fila, 1).GetValue<int>().Should().Be(1000 + (fila - 4));
            wsVentas.Cell(fila, 4).HasFormula.Should().BeFalse();
            wsVentas.Cell(fila, 5).GetString().Length.Should().BeLessThanOrEqualTo(32767);

            if (wsVentas.Cell(fila, 8).GetString() == "Pagada")
            {
                sumaEnExcelPagadas += wsVentas.Cell(fila, 11).GetValue<decimal>();
            }
        }

        sumaEnExcelPagadas.Should().Be(sumaEsperadaPagadas);

        // Verificar que la hoja de detalle tiene todas las líneas y truncó la nota de 33,000 caracteres sin fallar
        int ultimaFilaDetalle = wsDetalle.LastRowUsed()!.RowNumber();
        (ultimaFilaDetalle - 3).Should().Be(totalLineasProductoEsperadas);

        for (int fila = 4; fila <= ultimaFilaDetalle; fila++)
        {
            wsDetalle.Cell(fila, 10).GetString().Length.Should().BeLessThanOrEqualTo(32767);
        }
    }
}
