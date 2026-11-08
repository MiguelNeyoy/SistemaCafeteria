using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces.Services;
using Core.Domain.Enums;

namespace Core.Application.UseCases;

/// <summary>
/// Implementación con ClosedXML para exportar ventas filtradas y su desglose de artículos a formato .xlsx nativo.
/// </summary>
public class ExportadorVentasExcelService : IExportadorVentasExcelService
{
    private const int LimiteCaracteresCeldaExcel = 32700;
    private static readonly CultureInfo CulturaEsMx = new("es-MX");

    private static readonly XLColor ColorCafeEspresso = XLColor.FromHtml("#442A22");
    private static readonly XLColor ColorCremaSubtitulo = XLColor.FromHtml("#FFF8E1");
    private static readonly XLColor ColorFilaAlterna = XLColor.FromHtml("#FAFAFA");
    private static readonly XLColor ColorFilaCancelada = XLColor.FromHtml("#FFEBEE");
    private static readonly XLColor ColorTotalFondo = XLColor.FromHtml("#E8F5E9");
    private static readonly XLColor ColorBordeSuave = XLColor.FromHtml("#D7CCC8");

    public byte[] GenerarExcelVentas(
        IEnumerable<VentaResumenDto> ventas,
        string tituloPeriodo,
        string? resumenFiltros = null)
    {
        ArgumentNullException.ThrowIfNull(ventas);

        var listaVentas = ventas.ToList();
        string periodoLimpio = string.IsNullOrWhiteSpace(tituloPeriodo)
            ? "Historial de Ventas"
            : SanitizarTextoCelda(tituloPeriodo);

        using var workbook = new XLWorkbook();
        workbook.Style.Font.FontName = "Calibri";
        workbook.Style.Font.FontSize = 11;

        GenerarHojaVentas(workbook, listaVentas, periodoLimpio, resumenFiltros);
        GenerarHojaDetalleArticulos(workbook, listaVentas, periodoLimpio);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void GenerarHojaVentas(
        XLWorkbook workbook,
        List<VentaResumenDto> ventas,
        string periodoLimpio,
        string? resumenFiltros)
    {
        var ws = workbook.Worksheets.Add("Ventas");

        // Título principal
        var rangoTitulo = ws.Range("A1:K1");
        rangoTitulo.Merge();
        ws.Cell("A1").Value = "UNA MORDIDA — REPORTE DE VENTAS";
        rangoTitulo.Style.Fill.BackgroundColor = ColorCafeEspresso;
        rangoTitulo.Style.Font.FontColor = XLColor.White;
        rangoTitulo.Style.Font.Bold = true;
        rangoTitulo.Style.Font.FontSize = 14;
        rangoTitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        rangoTitulo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        ws.Row(1).Height = 28;

        // Subtítulo con periodo, fecha de emisión y filtros activos
        string infoFiltros = string.IsNullOrWhiteSpace(resumenFiltros)
            ? string.Empty
            : $"  |  Filtros: {SanitizarTextoCelda(resumenFiltros)}";

        var rangoSubtitulo = ws.Range("A2:K2");
        rangoSubtitulo.Merge();
        ws.Cell("A2").Value = $"Periodo: {periodoLimpio}{infoFiltros}  |  Emitido: {DateTime.Now.ToString("dd/MM/yyyy hh:mm tt", CulturaEsMx)}";
        rangoSubtitulo.Style.Fill.BackgroundColor = ColorCremaSubtitulo;
        rangoSubtitulo.Style.Font.FontColor = XLColor.FromHtml("#424242");
        rangoSubtitulo.Style.Font.FontSize = 10;
        rangoSubtitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(2).Height = 22;

        // Encabezados de tabla (Fila 4)
        const int filaHeader = 4;
        string[] encabezados =
        [
            "Folio",
            "Fecha",
            "Hora",
            "Cliente",
            "Productos",
            "Artículos",
            "Método Pago",
            "Estado",
            "Subtotal",
            "Descuento",
            "Total"
        ];

        for (int col = 0; col < encabezados.Length; col++)
        {
            var cell = ws.Cell(filaHeader, col + 1);
            cell.Value = encabezados[col];
            cell.Style.Fill.BackgroundColor = ColorCafeEspresso;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.Horizontal = col switch
            {
                0 or 1 or 2 or 5 or 6 or 7 => XLAlignmentHorizontalValues.Center,
                8 or 9 or 10 => XLAlignmentHorizontalValues.Right,
                _ => XLAlignmentHorizontalValues.Left
            };
        }
        ws.Row(filaHeader).Height = 24;

        // Filas de datos
        int filaActual = filaHeader + 1;
        foreach (var venta in ventas)
        {
            DateTime fechaRef = venta.FechaCierre ?? venta.FechaCreacion;
            string cliente = string.IsNullOrWhiteSpace(venta.IdentificadorCliente)
                ? "General"
                : SanitizarTextoCelda(venta.IdentificadorCliente);

            string resumenProductos = venta.Items.Count == 0
                ? "Sin productos"
                : string.Join("; ", venta.Items.Select(FormatearItemResumen));

            int cantidadArticulos = venta.Items.Sum(i => i.Cantidad);
            string metodoPago = venta.Estado == EstadoVenta.Pagado
                ? (venta.TipoDePago?.ToString() ?? "Efectivo")
                : "—";
            string estadoTexto = FormatearEstado(venta.Estado);

            ws.Cell(filaActual, 1).Value = venta.Id;
            ws.Cell(filaActual, 1).Style.NumberFormat.Format = "\"#\"0";
            ws.Cell(filaActual, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(filaActual, 1).Style.Font.Bold = true;

            ws.Cell(filaActual, 2).Value = fechaRef.Date;
            ws.Cell(filaActual, 2).Style.NumberFormat.Format = "dd/MM/yyyy";
            ws.Cell(filaActual, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(filaActual, 3).Value = fechaRef.ToString("hh:mm tt", CulturaEsMx);
            ws.Cell(filaActual, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(filaActual, 4).Value = cliente;
            ws.Cell(filaActual, 5).Value = TruncarLimiteExcel(SanitizarTextoCelda(resumenProductos));
            ws.Cell(filaActual, 5).Style.Alignment.WrapText = true;

            ws.Cell(filaActual, 6).Value = cantidadArticulos;
            ws.Cell(filaActual, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(filaActual, 7).Value = metodoPago;
            ws.Cell(filaActual, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(filaActual, 8).Value = estadoTexto;
            ws.Cell(filaActual, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(filaActual, 9).Value = venta.Subtotal;
            ws.Cell(filaActual, 9).Style.NumberFormat.Format = "$#,##0.00";

            ws.Cell(filaActual, 10).Value = venta.Descuento;
            ws.Cell(filaActual, 10).Style.NumberFormat.Format = "$#,##0.00";

            ws.Cell(filaActual, 11).Value = venta.Total;
            ws.Cell(filaActual, 11).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(filaActual, 11).Style.Font.Bold = true;

            var rangoFila = ws.Range(filaActual, 1, filaActual, 11);
            rangoFila.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            rangoFila.Style.Border.BottomBorderColor = ColorBordeSuave;
            rangoFila.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            if (venta.Estado is EstadoVenta.Cancelado or EstadoVenta.Devuelto)
            {
                rangoFila.Style.Fill.BackgroundColor = ColorFilaCancelada;
                rangoFila.Style.Font.FontColor = XLColor.FromHtml("#757575");
            }
            else if ((filaActual - filaHeader) % 2 == 0)
            {
                rangoFila.Style.Fill.BackgroundColor = ColorFilaAlterna;
            }

            filaActual++;
        }

        int ultimaFilaDatos = filaActual - 1;
        if (ventas.Count > 0)
        {
            ws.Range(filaHeader, 1, ultimaFilaDatos, 11).SetAutoFilter();
        }

        // Fila de Totales (solo ventas pagadas para cuadrar con el corte)
        int filaTotal = filaActual + 1;
        ws.Cell(filaTotal, 1).Value = "TOTALES (VENTAS PAGADAS)";
        ws.Range(filaTotal, 1, filaTotal, 4).Merge();
        ws.Cell(filaTotal, 5).Value = $"{ventas.Count(v => v.Estado == EstadoVenta.Pagado)} pagadas de {ventas.Count} registros";

        if (ventas.Count > 0)
        {
            ws.Cell(filaTotal, 6).FormulaA1 = $"SUMIFS(F5:F{ultimaFilaDatos},$H$5:$H${ultimaFilaDatos},\"Pagada\")";
            ws.Cell(filaTotal, 9).FormulaA1 = $"SUMIFS(I5:I{ultimaFilaDatos},$H$5:$H${ultimaFilaDatos},\"Pagada\")";
            ws.Cell(filaTotal, 10).FormulaA1 = $"SUMIFS(J5:J{ultimaFilaDatos},$H$5:$H${ultimaFilaDatos},\"Pagada\")";
            ws.Cell(filaTotal, 11).FormulaA1 = $"SUMIFS(K5:K{ultimaFilaDatos},$H$5:$H${ultimaFilaDatos},\"Pagada\")";
        }
        else
        {
            ws.Cell(filaTotal, 6).Value = 0;
            ws.Cell(filaTotal, 9).Value = 0m;
            ws.Cell(filaTotal, 10).Value = 0m;
            ws.Cell(filaTotal, 11).Value = 0m;
        }

        ws.Cell(filaTotal, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Cell(filaTotal, 9).Style.NumberFormat.Format = "$#,##0.00";
        ws.Cell(filaTotal, 10).Style.NumberFormat.Format = "$#,##0.00";
        ws.Cell(filaTotal, 11).Style.NumberFormat.Format = "$#,##0.00";

        var rangoTotal = ws.Range(filaTotal, 1, filaTotal, 11);
        rangoTotal.Style.Fill.BackgroundColor = ColorTotalFondo;
        rangoTotal.Style.Font.Bold = true;
        rangoTotal.Style.Border.TopBorder = XLBorderStyleValues.Medium;
        rangoTotal.Style.Border.BottomBorder = XLBorderStyleValues.Double;
        ws.Row(filaTotal).Height = 24;

        // Congelar encabezados y ajustar anchos de columnas de forma segura
        ws.SheetView.FreezeRows(filaHeader);
        AjustarColumnasSeguras(ws, new Dictionary<int, double>
        {
            [1] = 10,
            [2] = 13,
            [3] = 12,
            [4] = 24,
            [5] = 48,
            [6] = 11,
            [7] = 15,
            [8] = 13,
            [9] = 14,
            [10] = 14,
            [11] = 15
        });
    }

    private static void GenerarHojaDetalleArticulos(
        XLWorkbook workbook,
        List<VentaResumenDto> ventas,
        string periodoLimpio)
    {
        var ws = workbook.Worksheets.Add("Detalle Artículos");

        var rangoTitulo = ws.Range("A1:K1");
        rangoTitulo.Merge();
        ws.Cell("A1").Value = $"UNA MORDIDA — DETALLE POR ARTÍCULO ({periodoLimpio})";
        rangoTitulo.Style.Fill.BackgroundColor = ColorCafeEspresso;
        rangoTitulo.Style.Font.FontColor = XLColor.White;
        rangoTitulo.Style.Font.Bold = true;
        rangoTitulo.Style.Font.FontSize = 13;
        rangoTitulo.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        ws.Row(1).Height = 26;

        const int filaHeader = 3;
        string[] encabezados =
        [
            "Folio Venta",
            "Fecha",
            "Cliente",
            "Estado Venta",
            "Método Pago",
            "Cantidad",
            "Producto",
            "Precio Unitario",
            "Extras",
            "Notas",
            "Subtotal Línea"
        ];

        for (int col = 0; col < encabezados.Length; col++)
        {
            var cell = ws.Cell(filaHeader, col + 1);
            cell.Value = encabezados[col];
            cell.Style.Fill.BackgroundColor = ColorCafeEspresso;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.Horizontal = col switch
            {
                0 or 1 or 3 or 4 or 5 => XLAlignmentHorizontalValues.Center,
                7 or 10 => XLAlignmentHorizontalValues.Right,
                _ => XLAlignmentHorizontalValues.Left
            };
        }
        ws.Row(filaHeader).Height = 23;

        int filaActual = filaHeader + 1;
        foreach (var venta in ventas)
        {
            DateTime fechaRef = venta.FechaCierre ?? venta.FechaCreacion;
            string cliente = string.IsNullOrWhiteSpace(venta.IdentificadorCliente)
                ? "General"
                : SanitizarTextoCelda(venta.IdentificadorCliente);
            string estadoTexto = FormatearEstado(venta.Estado);
            string metodoPago = venta.Estado == EstadoVenta.Pagado
                ? (venta.TipoDePago?.ToString() ?? "Efectivo")
                : "—";

            foreach (var item in venta.Items)
            {
                string extrasTexto = item.Extras.Count == 0
                    ? string.Empty
                    : string.Join(", ", item.Extras.Select(e => $"{SanitizarTextoCelda(e.Nombre)} ({e.Precio.ToString("C", CulturaEsMx)})"));

                ws.Cell(filaActual, 1).Value = venta.Id;
                ws.Cell(filaActual, 1).Style.NumberFormat.Format = "\"#\"0";
                ws.Cell(filaActual, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(filaActual, 2).Value = fechaRef.Date;
                ws.Cell(filaActual, 2).Style.NumberFormat.Format = "dd/MM/yyyy";
                ws.Cell(filaActual, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(filaActual, 3).Value = cliente;

                ws.Cell(filaActual, 4).Value = estadoTexto;
                ws.Cell(filaActual, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(filaActual, 5).Value = metodoPago;
                ws.Cell(filaActual, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(filaActual, 6).Value = item.Cantidad;
                ws.Cell(filaActual, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(filaActual, 7).Value = TruncarLimiteExcel(SanitizarTextoCelda(item.ProductoNombre));

                ws.Cell(filaActual, 8).Value = item.PrecioUnitario;
                ws.Cell(filaActual, 8).Style.NumberFormat.Format = "$#,##0.00";

                ws.Cell(filaActual, 9).Value = TruncarLimiteExcel(extrasTexto);
                ws.Cell(filaActual, 10).Value = TruncarLimiteExcel(SanitizarTextoCelda(item.Notas));

                ws.Cell(filaActual, 11).Value = item.Subtotal;
                ws.Cell(filaActual, 11).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(filaActual, 11).Style.Font.Bold = true;

                var rangoFila = ws.Range(filaActual, 1, filaActual, 11);
                rangoFila.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rangoFila.Style.Border.BottomBorderColor = ColorBordeSuave;

                if (venta.Estado is EstadoVenta.Cancelado or EstadoVenta.Devuelto)
                {
                    rangoFila.Style.Fill.BackgroundColor = ColorFilaCancelada;
                    rangoFila.Style.Font.FontColor = XLColor.FromHtml("#757575");
                }
                else if ((filaActual - filaHeader) % 2 == 0)
                {
                    rangoFila.Style.Fill.BackgroundColor = ColorFilaAlterna;
                }

                filaActual++;
            }
        }

        if (filaActual > filaHeader + 1)
        {
            ws.Range(filaHeader, 1, filaActual - 1, 11).SetAutoFilter();
        }

        ws.SheetView.FreezeRows(filaHeader);
        AjustarColumnasSeguras(ws, new Dictionary<int, double>
        {
            [1] = 12,
            [2] = 13,
            [3] = 22,
            [4] = 14,
            [5] = 15,
            [6] = 11,
            [7] = 30,
            [8] = 15,
            [9] = 30,
            [10] = 25,
            [11] = 15
        });
    }

    private static string FormatearItemResumen(VentaItemDto item)
    {
        string nombre = SanitizarTextoCelda(item.ProductoNombre);
        if (item.Extras.Count == 0)
        {
            return $"{item.Cantidad}x {nombre}";
        }

        string extras = string.Join(", ", item.Extras.Select(e => $"+{SanitizarTextoCelda(e.Nombre)}"));
        return $"{item.Cantidad}x {nombre} ({extras})";
    }

    private static string FormatearEstado(EstadoVenta estado) => estado switch
    {
        EstadoVenta.Pagado => "Pagada",
        EstadoVenta.Cancelado => "Cancelada",
        EstadoVenta.Devuelto => "Devuelta",
        EstadoVenta.Pendiente => "Pendiente",
        _ => estado.ToString()
    };

    private static void AjustarColumnasSeguras(IXLWorksheet ws, Dictionary<int, double> anchosPorDefecto)
    {
        foreach (var (colIndex, anchoMinimo) in anchosPorDefecto)
        {
            var columna = ws.Column(colIndex);
            try
            {
                columna.AdjustToContents(3, 50);
                columna.Width = Math.Clamp(columna.Width + 2, anchoMinimo, 60);
            }
            catch
            {
                columna.Width = anchoMinimo;
            }
        }
    }

    private static string SanitizarTextoCelda(string? texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(texto.Length);
        foreach (char c in texto)
        {
            // OpenXML prohíbe caracteres de control ASCII < 0x20 salvo \t, \n, \r, y caracteres Unicode inválidos
            if (c == '\r' || c == '\n' || c == '\t')
            {
                sb.Append(' ');
                continue;
            }

            if (char.IsControl(c) || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Format)
            {
                continue;
            }

            sb.Append(c);
        }

        return sb.ToString().Trim();
    }

    private static string TruncarLimiteExcel(string texto)
    {
        if (texto.Length <= LimiteCaracteresCeldaExcel)
        {
            return texto;
        }

        return texto[..(LimiteCaracteresCeldaExcel - 3)] + "...";
    }
}
