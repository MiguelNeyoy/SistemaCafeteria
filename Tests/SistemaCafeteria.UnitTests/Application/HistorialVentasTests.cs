using Core.Application.Dtos.Ventas;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.UseCases;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace SistemaCafeteria.UnitTests.Application;

public class HistorialVentasTests
{
    private readonly Mock<IVentaRepository> _ventaRepoMock = new();
    private readonly Mock<IProductoRepository> _productoRepoMock = new();
    private readonly Mock<IExtraRepository> _extraRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly VentaService _ventaService;
    private readonly CorteCajaService _corteCajaService;

    public HistorialVentasTests()
    {
        _ventaService = new VentaService(
            _ventaRepoMock.Object,
            _productoRepoMock.Object,
            _extraRepoMock.Object,
            _unitOfWorkMock.Object);

        _corteCajaService = new CorteCajaService(_ventaRepoMock.Object);
    }

    [Fact]
    public async Task ObtenerPorRangoFechasAsync_ConMasDe10Ventas_RetornaTodasSinLimiteOrdenadasDescendente()
    {
        // Arrange: 25 ventas en distintos horarios del día
        var baseFecha = new DateTime(2026, 11, 7, 8, 0, 0);
        var ventas = new List<Venta>();

        for (int i = 1; i <= 25; i++)
        {
            var item = new VentaItem(1, "Americano", 45m, 1);
            var venta = new Venta(
                id: i,
                identificadorCliente: $"Mesa {i}",
                fechaCreacion: baseFecha.AddMinutes(i * 10),
                fechaCierre: baseFecha.AddMinutes(i * 10 + 5),
                estado: EstadoVenta.Pagado,
                tipoDePago: TipoDePago.Efectivo,
                descuento: 0m,
                montoRecibido: 50m,
                items: new[] { item });
            ventas.Add(venta);
        }

        _ventaRepoMock
            .Setup(r => r.ObtenerPorRangoFechasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(ventas);

        // Act
        var resultado = await _ventaService.ObtenerPorRangoFechasAsync(baseFecha, baseFecha);

        // Assert
        resultado.Should().HaveCount(25);
        resultado.First().Id.Should().Be(25);
        resultado.Last().Id.Should().Be(1);
    }

    [Fact]
    public async Task ObtenerPorRangoFechasAsync_FechaFinalAnteriorAInicial_LanzaDomainValidationException()
    {
        var desde = new DateTime(2026, 11, 10);
        var hasta = new DateTime(2026, 11, 5);

        Func<Task> act = async () => await _ventaService.ObtenerPorRangoFechasAsync(desde, hasta);

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task ModificarDatosPagoVentaAsync_CambioDeEfectivoATransferenciaYPreformateoCliente_ActualizaCorrectamente()
    {
        // Arrange
        var item = new VentaItem(10, "Chilaquiles Verdes", 115m, 2, "Sin cebolla", new[]
        {
            new VentaItemExtra(1, "Huevo Estrellado", 20m)
        });

        var venta = new Venta(
            id: 42,
            identificadorCliente: "General",
            fechaCreacion: DateTime.Today.AddHours(9),
            fechaCierre: DateTime.Today.AddHours(9).AddMinutes(25),
            estado: EstadoVenta.Pagado,
            tipoDePago: TipoDePago.Efectivo,
            descuento: 0m,
            montoRecibido: 300m,
            items: new[] { item });

        _ventaRepoMock.Setup(r => r.ObtenerPorIdConDetallesAsync(42)).ReturnsAsync(venta);

        // Act: El cajero corrige que fue Transferencia, aplica $20 de descuento olvidado y corrige nombre con ñ/espacios
        var actualizada = await _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
        {
            VentaId = 42,
            IdentificadorCliente = "   maría   de la luz   ñuñez  ",
            TipoDePago = TipoDePago.Transferencia,
            Descuento = 20m
        });

        // Assert: Subtotal = (115 + 20) * 2 = 270; Total = 270 - 20 = 250
        actualizada.IdentificadorCliente.Should().Be("María de la Luz Ñuñez");
        actualizada.TipoDePago.Should().Be(TipoDePago.Transferencia);
        actualizada.Subtotal.Should().Be(270m);
        actualizada.Descuento.Should().Be(20m);
        actualizada.Total.Should().Be(250m);
        actualizada.MontoRecibido.Should().BeNull();
        actualizada.Cambio.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ModificarDatosPagoVentaAsync_ValidacionesContraBlancosNegativosYDatosErroneos_BloqueaCambiosInvalidos()
    {
        var item = new VentaItem(1, "Latte", 65m, 2); // Subtotal = 130
        var ventaPagada = new Venta(
            id: 10,
            identificadorCliente: "Carlos Ruiz",
            fechaCreacion: DateTime.Now.AddMinutes(-20),
            fechaCierre: DateTime.Now.AddMinutes(-5),
            estado: EstadoVenta.Pagado,
            tipoDePago: TipoDePago.Tarjeta,
            descuento: 0m,
            montoRecibido: null,
            items: new[] { item });

        _ventaRepoMock.Setup(r => r.ObtenerPorIdConDetallesAsync(10)).ReturnsAsync(ventaPagada);

        // 1. Nombre en blanco o espacios/tabuladores
        Func<Task> nombreBlanco = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
        {
            VentaId = 10,
            IdentificadorCliente = "   \t\r\n  ",
            TipoDePago = TipoDePago.Tarjeta
        });
        await nombreBlanco.Should().ThrowAsync<DomainValidationException>();

        // 2. Descuento negativo
        Func<Task> descuentoNegativo = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
        {
            VentaId = 10,
            IdentificadorCliente = "Carlos Ruiz",
            TipoDePago = TipoDePago.Tarjeta,
            Descuento = -15m
        });
        await descuentoNegativo.Should().ThrowAsync<DomainValidationException>();

        // 3. Descuento mayor al subtotal ($130)
        Func<Task> descuentoExcesivo = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
        {
            VentaId = 10,
            IdentificadorCliente = "Carlos Ruiz",
            TipoDePago = TipoDePago.Tarjeta,
            Descuento = 130.01m
        });
        await descuentoExcesivo.Should().ThrowAsync<DomainValidationException>();

        // 4. Monto recibido negativo
        Func<Task> recibidoNegativo = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
        {
            VentaId = 10,
            IdentificadorCliente = "Carlos Ruiz",
            TipoDePago = TipoDePago.Efectivo,
            MontoRecibido = -100m
        });
        await recibidoNegativo.Should().ThrowAsync<DomainValidationException>();

        // 5. Monto recibido en efectivo insuficiente (< $130)
        Func<Task> recibidoInsuficiente = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
        {
            VentaId = 10,
            IdentificadorCliente = "Carlos Ruiz",
            TipoDePago = TipoDePago.Efectivo,
            MontoRecibido = 100m
        });
        await recibidoInsuficiente.Should().ThrowAsync<DomainValidationException>();

        // 6. Enum TipoDePago inválido / corrupto
        Func<Task> enumInvalido = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
        {
            VentaId = 10,
            IdentificadorCliente = "Carlos Ruiz",
            TipoDePago = (TipoDePago)999
        });
        await enumInvalido.Should().ThrowAsync<DomainValidationException>();

        // 7. Cuenta cancelada no debe permitir edición
        var ventaCancelada = new Venta("Mesa Cancelada");
        ventaCancelada.Cancelar();
        _ventaRepoMock.Setup(r => r.ObtenerPorIdConDetallesAsync(99)).ReturnsAsync(ventaCancelada);

        Func<Task> editarCancelada = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
        {
            VentaId = 99,
            IdentificadorCliente = "Intento",
            TipoDePago = TipoDePago.Efectivo,
            MontoRecibido = 100m
        });
        await editarCancelada.Should().ThrowAsync<DomainException>();

        // Verificar que la venta original #10 permaneció intacta tras todos los intentos fallidos
        ventaPagada.IdentificadorCliente.Should().Be("Carlos Ruiz");
        ventaPagada.TipoDePago.Should().Be(TipoDePago.Tarjeta);
        ventaPagada.Total.Should().Be(130m);
    }

    [Fact]
    public async Task PruebaDeCaos_100VentasConCorreccionesMasivasDeMetodosDePagoYIntentosCorruptos_CuadraCorteAlCentavo()
    {
        // Arrange: Simular una jornada intensa de 100 transacciones (85 pagadas, 10 canceladas, 5 devueltas)
        var fechaJornada = new DateTime(2026, 11, 7, 8, 0, 0);
        var bdMemoria = new Dictionary<int, Venta>();

        for (int id = 1; id <= 100; id++)
        {
            var items = new List<VentaItem>
            {
                new(productoId: (id % 10) + 1, productoNombre: $"Platillo #{id % 10}", precioUnitario: 62.50m + (id % 5) * 7.25m, cantidad: (id % 3) + 1)
            };

            if (id <= 85)
            {
                // Inicialmente muchos fueron cobrados erróneamente en Efectivo
                var tipoInicial = (id % 4 == 0) ? TipoDePago.Tarjeta
                    : (id % 5 == 0) ? TipoDePago.Transferencia
                    : TipoDePago.Efectivo;

                decimal subtotal = items.Sum(i => i.Subtotal);
                decimal descuentoInicial = (id % 10 == 0) ? 10m : 0m;
                decimal totalInicial = subtotal - descuentoInicial;

                bdMemoria[id] = new Venta(
                    id: id,
                    identificadorCliente: $"cliente #{id}",
                    fechaCreacion: fechaJornada.AddMinutes(id * 4),
                    fechaCierre: fechaJornada.AddMinutes(id * 4 + 12),
                    estado: EstadoVenta.Pagado,
                    tipoDePago: tipoInicial,
                    descuento: descuentoInicial,
                    montoRecibido: tipoInicial == TipoDePago.Efectivo ? totalInicial + 50m : null,
                    items: items);
            }
            else if (id <= 95)
            {
                bdMemoria[id] = new Venta(
                    id: id,
                    identificadorCliente: $"Cancelada #{id}",
                    fechaCreacion: fechaJornada.AddMinutes(id * 4),
                    fechaCierre: fechaJornada.AddMinutes(id * 4 + 3),
                    estado: EstadoVenta.Cancelado,
                    tipoDePago: null,
                    descuento: 0m,
                    montoRecibido: null,
                    items: items);
            }
            else
            {
                decimal subtotal = items.Sum(i => i.Subtotal);
                bdMemoria[id] = new Venta(
                    id: id,
                    identificadorCliente: $"Devuelta #{id}",
                    fechaCreacion: fechaJornada.AddMinutes(id * 4),
                    fechaCierre: fechaJornada.AddMinutes(id * 4 + 8),
                    estado: EstadoVenta.Devuelto,
                    tipoDePago: TipoDePago.Efectivo,
                    descuento: 0m,
                    montoRecibido: subtotal,
                    items: items);
            }
        }

        _ventaRepoMock
            .Setup(r => r.ObtenerPorIdConDetallesAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => bdMemoria.TryGetValue(id, out var v) ? v : null);

        _ventaRepoMock
            .Setup(r => r.ObtenerPorFechaAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(() => bdMemoria.Values.ToList());

        // Act: Ráfaga de auditoría de corte de caja (corregir 30 cuentas mal registradas + intercalar entradas corruptas)
        for (int id = 1; id <= 30; id++)
        {
            var nuevoMetodo = (id % 3) switch
            {
                0 => TipoDePago.Transferencia,
                1 => TipoDePago.Tarjeta,
                _ => TipoDePago.Efectivo
            };

            decimal subtotalVenta = bdMemoria[id].Subtotal;
            decimal nuevoDescuento = (id % 6 == 0) ? 12.50m : 0m;
            decimal nuevoTotal = subtotalVenta - nuevoDescuento;

            await _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
            {
                VentaId = id,
                IdentificadorCliente = $"  mesa   {id} - peña  y  asociados ",
                TipoDePago = nuevoMetodo,
                Descuento = nuevoDescuento,
                MontoRecibido = nuevoMetodo == TipoDePago.Efectivo ? nuevoTotal + 20m : null
            });

            // Intentos caóticos inválidos intercalados que NO deben corromper el estado
            Func<Task> intentoCorrupto = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
            {
                VentaId = id,
                IdentificadorCliente = (id % 2 == 0) ? "   " : "Valido",
                TipoDePago = nuevoMetodo,
                Descuento = (id % 2 == 0) ? 0m : -99.99m
            });
            await intentoCorrupto.Should().ThrowAsync<DomainValidationException>();
        }

        // Intentar alterar cuentas canceladas/devueltas (86..100)
        for (int id = 86; id <= 100; id++)
        {
            Func<Task> intentoNoEditable = () => _ventaService.ModificarDatosPagoVentaAsync(new ModificarVentaDto
            {
                VentaId = id,
                IdentificadorCliente = "Intento Ilegal",
                TipoDePago = TipoDePago.Tarjeta
            });
            await intentoNoEditable.Should().ThrowAsync<DomainException>();
        }

        // Generar el Corte Diario después de toda la auditoría
        var corteFinal = await _corteCajaService.GenerarCorteDiarioAsync(fechaJornada, fondoInicial: 500m);

        // Assert: Exactitud financiera al centavo
        var pagadasReales = bdMemoria.Values.Where(v => v.Estado == EstadoVenta.Pagado).ToList();
        pagadasReales.Should().HaveCount(85);
        pagadasReales.Should().NotContain(v => string.IsNullOrWhiteSpace(v.IdentificadorCliente));
        pagadasReales.Should().NotContain(v => v.Descuento < 0m || v.Total < 0m);

        decimal sumaEfectivoEsperada = pagadasReales.Where(v => v.TipoDePago == TipoDePago.Efectivo).Sum(v => v.Total);
        decimal sumaTarjetaEsperada = pagadasReales.Where(v => v.TipoDePago == TipoDePago.Tarjeta).Sum(v => v.Total);
        decimal sumaTransferenciaEsperada = pagadasReales.Where(v => v.TipoDePago == TipoDePago.Transferencia).Sum(v => v.Total);
        decimal sumaDescuentosEsperada = pagadasReales.Sum(v => v.Descuento);

        corteFinal.CantidadVentas.Should().Be(85);
        corteFinal.CantidadCanceladas.Should().Be(10);
        corteFinal.CantidadDevueltas.Should().Be(5);

        corteFinal.TotalEfectivo.Should().Be(sumaEfectivoEsperada);
        corteFinal.TotalTarjeta.Should().Be(sumaTarjetaEsperada);
        corteFinal.TotalTransferencia.Should().Be(sumaTransferenciaEsperada);
        corteFinal.TotalDescuentos.Should().Be(sumaDescuentosEsperada);
        corteFinal.TotalVentas.Should().Be(sumaEfectivoEsperada + sumaTarjetaEsperada + sumaTransferenciaEsperada);
        corteFinal.EsperadoEnCajon.Should().Be(500m + sumaEfectivoEsperada - corteFinal.TotalDevoluciones);
    }
}
