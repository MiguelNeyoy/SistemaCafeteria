using Core.Application.Dtos.Comandas;
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

public class IdentificadorClienteTests
{
    private readonly Mock<IVentaRepository> _ventaRepoMock = new();
    private readonly Mock<IProductoRepository> _productoRepoMock = new();
    private readonly Mock<IExtraRepository> _extraRepoMock = new();
    private readonly Mock<IComandaRepository> _comandaRepoMock = new();
    private readonly Mock<ICategoriaRepository> _categoriaRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly VentaService _ventaService;
    private readonly ComandaService _comandaService;

    public IdentificadorClienteTests()
    {
        _ventaService = new VentaService(
            _ventaRepoMock.Object,
            _productoRepoMock.Object,
            _extraRepoMock.Object,
            _unitOfWorkMock.Object);

        _comandaService = new ComandaService(
            _comandaRepoMock.Object,
            _ventaRepoMock.Object,
            _productoRepoMock.Object,
            _categoriaRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CrearCuentaAsync_RechazaIdentificadorVacioOEnBlancoSinAsignarGeneral()
    {
        Func<Task> actVacio = () => _ventaService.CrearCuentaAsync("");
        Func<Task> actEspacios = () => _ventaService.CrearCuentaAsync("   \t\u200B  ");

        await actVacio.Should().ThrowAsync<DomainValidationException>();
        await actEspacios.Should().ThrowAsync<DomainValidationException>();

        _ventaRepoMock.Verify(r => r.AgregarAsync(It.IsAny<Venta>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CrearCuentaAsync_AplicaPreformateoTitleCaseYPersisteIdentificador()
    {
        // Act
        var dto = await _ventaService.CrearCuentaAsync("   mesa 4 - josé   de la cruz   ");

        // Assert
        dto.IdentificadorCliente.Should().Be("Mesa 4 - José de la Cruz");
        _ventaRepoMock.Verify(r => r.AgregarAsync(It.Is<Venta>(v => v.IdentificadorCliente == "Mesa 4 - José de la Cruz")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarACocinaAsync_PropagaIdentificadorFormateadoDeLaVentaALaComanda()
    {
        // Arrange
        var ventaCreada = new Venta("  terraza 2 / maría ñuñez  ");
        var ventaPersistida = new Venta(
            id: 7,
            identificadorCliente: ventaCreada.IdentificadorCliente,
            fechaCreacion: ventaCreada.FechaCreacion,
            fechaCierre: null,
            estado: EstadoVenta.Pendiente,
            tipoDePago: null,
            descuento: 0m,
            montoRecibido: null);
        var producto = new Producto(id: 1, nombre: "Capuchino", precio: 55m, categoriaId: 1);
        var categoria = new Categoria("Café & Capp");

        _ventaRepoMock.Setup(r => r.ObtenerPorIdAsync(7)).ReturnsAsync(ventaPersistida);
        _productoRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(producto);
        _categoriaRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(categoria);

        // Act
        var comandaDto = await _comandaService.EnviarACocinaAsync(7, new List<EnviarComandaItemDto>
        {
            new() { ProductoId = 1, Cantidad = 2 }
        });

        // Assert
        comandaDto.IdentificadorCliente.Should().Be("Terraza 2 / María Ñuñez");
    }

    [Fact]
    public async Task PruebaDeCaos_50EntradasExtremasConBlancosInvisiblesBilinguesYEspeciales_RechazaInvalidasYPreformatea100PorCiento()
    {
        // 1. Batería de 20 entradas inválidas (nulas, vacías, controles, espacios Unicode invisibles y desbordamientos > 100 chars)
        var entradasInvalidas = new string?[]
        {
            null,
            "",
            " ",
            "     ",
            "\t",
            "\r\n",
            " \t \r \n ",
            "\u200B", // Zero-Width Space
            "\uFEFF", // BOM / Zero-Width No-Break Space
            "\u2060", // Word Joiner
            "\u200B   \uFEFF \t \n",
            "\u00A0\u00A0\u00A0", // Non-breaking spaces
            "\u2000\u2001\u2002\u2003", // En/Em quads & spaces
            "\0\0\0",
            "\u0007\u001B", // Bell & Escape control chars
            new string(' ', 250),
            new string('A', 101),
            "Mesa " + new string('X', 100),
            string.Join(" ", Enumerable.Repeat(" PalabraLarga ", 15)),
            "   \u200C\u200D   "
        };

        foreach (var invalida in entradasInvalidas)
        {
            Func<Task> actVenta = () => _ventaService.CrearCuentaAsync(invalida!);
            await actVenta.Should().ThrowAsync<DomainValidationException>();

            Action actComanda = () => _ = new Comanda(
                ventaId: 1,
                identificadorCliente: invalida!,
                items: new[] { new ComandaItem(1, "Latte", 1, rol: RolComanda.Barista) });
            actComanda.Should().Throw<DomainValidationException>();
        }

        // 2. Batería de 30 entradas válidas caóticas reales de cafetería (español, inglés, tildes, diéresis, ñ, delimitadores)
        var casosValidos = new (string Entrada, string Esperado)[]
        {
            ("juan", "Juan"),
            ("   SOFÍA   ", "Sofía"),
            ("mesa   12", "Mesa 12"),
            ("   para   llevar   ", "Para Llevar"),
            ("maría   del   carmen   ñuñez", "María del Carmen Ñuñez"),
            ("PEÑA   Y   ASOCIADOS", "Peña y Asociados"),
            ("mesa 3 - ana de la torre", "Mesa 3 - Ana de la Torre"),
            ("mesa 5 / terraza (sin prisa)", "Mesa 5 / Terraza (Sin Prisa)"),
            ("john   and   mary   with   kids", "John and Mary with Kids"),
            ("cup   of   coffee   in   bar", "Cup of Coffee in Bar"),
            ("pingüino   güero", "Pingüino Güero"),
            ("ÓSCAR   ÁLVAREZ   ÍÑIGUEZ", "Óscar Álvarez Íñiguez"),
            ("barra #2", "Barra #2"),
            ("uber-eats:   pedido   #409", "Uber-Eats: Pedido #409"),
            ("rappi / jorge lópez", "Rappi / Jorge López"),
            ("didi (cliente: raúl)", "Didi (Cliente: Raúl)"),
            ("  \t\u200B  lucía   de   los   ángeles \r\n ", "Lucía de los Ángeles"),
            ("señorita   cañedo", "Señorita Cañedo"),
            ("NIÑO   DE   RIVERA", "Niño de Rivera"),
            ("mesa 16oz", "Mesa 16oz"),
            ("2x1 promo mesa 8", "2x1 Promo Mesa 8"),
            ("familia o'connor", "Familia O'connor"),
            ("[vip]   carlos   slim", "[Vip] Carlos Slim"),
            ("\"el   profe\"   martínez", "\"El Profe\" Martínez"),
            ("terraza - esquina con sombra", "Terraza - Esquina con Sombra"),
            ("camioneta   blanca   afuera", "Camioneta Blanca Afuera"),
            ("doctora   muñoz   hospital", "Doctora Muñoz Hospital"),
            ("grupo   de   bicicletas   (8   personas)", "Grupo de Bicicletas (8 Personas)"),
            ("para llevar: bolsa aparte", "Para Llevar: Bolsa Aparte"),
            (new string('a', 100), "A" + new string('a', 99)) // Exactamente en el límite permitido de 100 caracteres
        };

        var creadas = new List<string>();
        foreach (var (entrada, esperado) in casosValidos)
        {
            var resumen = await _ventaService.CrearCuentaAsync(entrada);
            resumen.IdentificadorCliente.Should().Be(esperado);

            var comanda = new Comanda(
                ventaId: 1,
                identificadorCliente: entrada,
                items: new[] { new ComandaItem(1, "Capuchino", 1, rol: RolComanda.Barista) });
            comanda.IdentificadorCliente.Should().Be(esperado);

            creadas.Add(resumen.IdentificadorCliente);
        }

        // Assert global: las 30 válidas quedaron limpias, reconocibles y ninguna cayó en blanco ni en "General" automático
        creadas.Should().HaveCount(30);
        creadas.Should().NotContain(c => string.IsNullOrWhiteSpace(c));
        creadas.Should().NotContain("General");
    }
}
