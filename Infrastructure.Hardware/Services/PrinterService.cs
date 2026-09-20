using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Dtos.Comandas;
using Core.Application.Dtos.Reportes;
using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Domain.Enums;

namespace Infrastructure.Hardware.Services;

public class PrinterService : IPrinterService
{
    private readonly IConfiguracionRepository _configuracionRepository;

    public const string KeyImpresoraTickets = "Impresora_NombreTicket";
    public const string KeyImpresoraComandas = "Impresora_NombreComanda";
    public const string KeyAnchoPapel = "Impresora_AnchoPapel";
    public const string KeyAbrirCajon = "Impresora_AbrirCajon";
    public const string KeyCortarPapel = "Impresora_CortarPapel";

    public PrinterService(IConfiguracionRepository configuracionRepository)
    {
        _configuracionRepository = configuracionRepository;
    }

    public List<string> ObtenerImpresorasInstaladas()
    {
        return RawPrinterHelper.ObtenerImpresorasInstaladas();
    }

    public async Task AbrirCajonDineroAsync()
    {
        string? impresora = await _configuracionRepository.ObtenerValorAsync(KeyImpresoraTickets);
        if (string.IsNullOrWhiteSpace(impresora))
        {
            throw new InvalidOperationException("No se ha configurado ninguna impresora térmica para el cajón de dinero. Ve a Configuración Avanzada.");
        }

        var builder = new EscPosBuilder().AbrirCajon();
        await Task.Run(() => RawPrinterHelper.EnviarBytes(impresora, builder.Construir(), "Abrir Cajon"));
    }

    public async Task ImprimirTicketAsync(VentaResumenDto venta, string folio)
    {
        ArgumentNullException.ThrowIfNull(venta);

        string? impresora = await _configuracionRepository.ObtenerValorAsync(KeyImpresoraTickets);
        if (string.IsNullOrWhiteSpace(impresora))
        {
            throw new InvalidOperationException("No se ha configurado la impresora térmica de tickets. Configure una en Configuración Avanzada.");
        }

        string ancho = await _configuracionRepository.ObtenerValorAsync(KeyAnchoPapel) ?? "58mm";
        bool abrirCajon = (await _configuracionRepository.ObtenerValorAsync(KeyAbrirCajon) ?? "true") == "true";
        bool cortarPapel = (await _configuracionRepository.ObtenerValorAsync(KeyCortarPapel) ?? "true") == "true";

        var builder = new EscPosBuilder(ancho);

        // Si fue en efectivo y está configurado, mandar pulso para abrir cajón
        if (abrirCajon && venta.TipoDePago == TipoDePago.Efectivo)
        {
            builder.AbrirCajon();
        }

        // Encabezado
        builder.AlinearCentro()
               .TamanoGrande()
               .Negrita(true)
               .Linea("UNA MORDIDA")
               .TamanoNormal()
               .Negrita(false)
               .Linea("Cafeteria & Antojitos")
               .Linea("Comprobante de Venta")
               .LineaSeparadora()
               .AlinearIzquierda()
               .Fila2Columnas("Folio:", $"#{folio}")
               .Fila2Columnas("Fecha:", (venta.FechaCierre ?? venta.FechaCreacion).ToString("dd/MM/yyyy HH:mm"))
               .Fila2Columnas("Cliente:", venta.IdentificadorCliente ?? "General")
               .LineaSeparadora()
               .Negrita(true)
               .Fila2Columnas("Cant  Descripcion", "Importe")
               .Negrita(false)
               .LineaSeparadora();

        // Lista de productos
        foreach (var item in venta.Items)
        {
            builder.FilaProducto(item.Cantidad, item.ProductoNombre, item.Subtotal);

            if (item.Extras != null)
            {
                foreach (var extra in item.Extras)
                {
                    builder.ItemExtra(extra.Nombre, extra.Precio * item.Cantidad);
                }
            }

            if (!string.IsNullOrWhiteSpace(item.Notas))
            {
                builder.Nota(item.Notas);
            }
        }

        builder.LineaSeparadora();

        // Totales
        builder.Fila2Columnas("Subtotal:", venta.Subtotal.ToString("C2"));
        if (venta.Descuento > 0)
        {
            builder.Fila2Columnas("Descuento:", $"-{venta.Descuento:C2}");
        }

        builder.Negrita(true)
               .TamanoDobleAlto()
               .Fila2Columnas("TOTAL:", venta.Total.ToString("C2"))
               .TamanoNormal()
               .Negrita(false)
               .LineaSeparadora();

        // Desglose de pago
        builder.Fila2Columnas("Forma de Pago:", venta.TipoDePago?.ToString() ?? "Efectivo");
        if (venta.TipoDePago == TipoDePago.Efectivo)
        {
            builder.Fila2Columnas("Recibido:", (venta.MontoRecibido ?? 0m).ToString("C2"));
            builder.Fila2Columnas("Cambio:", (venta.Cambio ?? 0m).ToString("C2"));
        }

        builder.LineaSeparadora()
               .AlinearCentro()
               .Linea("Gracias por su preferencia!")
               .Linea("Vuelva pronto");

        if (cortarPapel)
        {
            builder.CortarPapel();
        }
        else
        {
            builder.AlimentarLineas(4);
        }

        byte[] payload = builder.Construir();
        await Task.Run(() => RawPrinterHelper.EnviarBytes(impresora, payload, $"Ticket #{folio}"));
    }

    public async Task ImprimirComandaAsync(ComandaResumenDto comanda)
    {
        ArgumentNullException.ThrowIfNull(comanda);

        string? impresora = await _configuracionRepository.ObtenerValorAsync(KeyImpresoraComandas);
        if (string.IsNullOrWhiteSpace(impresora))
        {
            impresora = await _configuracionRepository.ObtenerValorAsync(KeyImpresoraTickets);
        }

        if (string.IsNullOrWhiteSpace(impresora))
        {
            throw new InvalidOperationException("No se ha configurado ninguna impresora para comandas de cocina.");
        }

        string ancho = await _configuracionRepository.ObtenerValorAsync(KeyAnchoPapel) ?? "58mm";
        bool cortarPapel = (await _configuracionRepository.ObtenerValorAsync(KeyCortarPapel) ?? "true") == "true";

        var builder = new EscPosBuilder(ancho);

        builder.AlinearCentro()
               .TamanoGrande()
               .Negrita(true)
               .Linea("*** COCINA ***")
               .TamanoDobleAlto()
               .Linea($"Comanda #{comanda.Id}")
               .TamanoNormal()
               .Linea($"Cliente: {comanda.IdentificadorCliente ?? "General"}")
               .Linea($"Hora: {comanda.FechaCreacion:HH:mm:ss}")
               .LineaSeparadora('=')
               .AlinearIzquierda();

        foreach (var item in comanda.Items)
        {
            builder.TamanoDobleAlto()
                   .Negrita(true)
                   .Linea($"{item.Cantidad}x  {item.ProductoNombre}")
                   .TamanoNormal()
                   .Negrita(false);

            if (item.Extras != null)
            {
                foreach (var extra in item.Extras)
                {
                    builder.Linea($"   + {extra}");
                }
            }

            if (!string.IsNullOrWhiteSpace(item.NotasCocina))
            {
                builder.Negrita(true)
                       .Linea($"   * NOTA: {item.NotasCocina}")
                       .Negrita(false);
            }

            builder.Linea();
        }

        builder.LineaSeparadora('=');

        if (cortarPapel)
        {
            builder.CortarPapel();
        }
        else
        {
            builder.AlimentarLineas(4);
        }

        byte[] payload = builder.Construir();
        await Task.Run(() => RawPrinterHelper.EnviarBytes(impresora, payload, $"Comanda #{comanda.Id}"));
    }

    public async Task ImprimirCorteCajaAsync(CorteCajaDto corte)
    {
        ArgumentNullException.ThrowIfNull(corte);

        string? impresora = await _configuracionRepository.ObtenerValorAsync(KeyImpresoraTickets);
        if (string.IsNullOrWhiteSpace(impresora))
        {
            throw new InvalidOperationException("No se ha configurado ninguna impresora para imprimir el corte de caja.");
        }

        string ancho = await _configuracionRepository.ObtenerValorAsync(KeyAnchoPapel) ?? "58mm";
        bool cortarPapel = (await _configuracionRepository.ObtenerValorAsync(KeyCortarPapel) ?? "true") == "true";

        var builder = new EscPosBuilder(ancho);

        builder.AlinearCentro()
               .TamanoGrande()
               .Negrita(true)
               .Linea("UNA MORDIDA")
               .TamanoNormal()
               .Linea("CORTE DE CAJA")
               .LineaSeparadora()
               .AlinearIzquierda()
               .Fila2Columnas("Desde:", corte.FechaInicio.ToString("dd/MM/yyyy HH:mm"))
               .Fila2Columnas("Hasta:", corte.FechaFin.ToString("dd/MM/yyyy HH:mm"))
               .LineaSeparadora()
               .Fila2Columnas("Fondo Inicial:", corte.FondoInicial.ToString("C2"))
               .Fila2Columnas("Ventas Efectivo:", corte.TotalEfectivo.ToString("C2"))
               .Fila2Columnas("Ventas Tarjeta:", corte.TotalTarjeta.ToString("C2"))
               .Fila2Columnas("Transferencia:", corte.TotalTransferencia.ToString("C2"))
               .LineaSeparadora()
               .Negrita(true)
               .Fila2Columnas("TOTAL VENTAS:", corte.TotalVentas.ToString("C2"))
               .Negrita(false);

        if (corte.TotalDescuentos > 0)
        {
            builder.Fila2Columnas("Descuentos:", corte.TotalDescuentos.ToString("C2"));
        }
        if (corte.TotalDevoluciones > 0)
        {
            builder.Fila2Columnas("Devoluciones:", corte.TotalDevoluciones.ToString("C2"));
        }

        builder.LineaSeparadora('=')
               .Negrita(true)
               .TamanoDobleAlto()
               .Fila2Columnas("EN CAJON:", corte.EsperadoEnCajon.ToString("C2"))
               .TamanoNormal()
               .Negrita(false)
               .LineaSeparadora('=')
               .AlinearCentro()
               .Linea($"Impreso: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

        if (cortarPapel)
        {
            builder.CortarPapel();
        }
        else
        {
            builder.AlimentarLineas(4);
        }

        byte[] payload = builder.Construir();
        await Task.Run(() => RawPrinterHelper.EnviarBytes(impresora, payload, "Corte de Caja"));
    }

    public async Task ImprimirTicketPruebaAsync(string nombreImpresora, string anchoPapel)
    {
        if (string.IsNullOrWhiteSpace(nombreImpresora))
        {
            throw new InvalidOperationException("Debes seleccionar una impresora para la prueba.");
        }

        var builder = new EscPosBuilder(anchoPapel);

        builder.AlinearCentro()
               .TamanoGrande()
               .Negrita(true)
               .Linea("UNA MORDIDA")
               .TamanoNormal()
               .Negrita(false)
               .Linea("TICKET DE PRUEBA")
               .LineaSeparadora()
               .AlinearIzquierda()
               .Fila2Columnas("Impresora:", nombreImpresora)
               .Fila2Columnas("Ancho:", anchoPapel)
               .Fila2Columnas("Fecha:", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
               .LineaSeparadora()
               .AlinearCentro()
               .Linea("Prueba de Acentos y Caracteres:")
               .Linea("á, é, í, ó, ú, ñ, Ñ, $, ¿?")
               .LineaSeparadora()
               .Linea("Impresion termica lista!")
               .CortarPapel();

        byte[] payload = builder.Construir();
        await Task.Run(() => RawPrinterHelper.EnviarBytes(nombreImpresora, payload, "Ticket de Prueba"));
    }
}
