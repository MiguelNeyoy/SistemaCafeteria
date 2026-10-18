using System.Reflection;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.UseCases;
using Core.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace SistemaCafeteria.UnitTests.Application;

public class BusquedaComandaTests
{
    private readonly Mock<IProductoRepository> _productoRepoMock = new();
    private readonly Mock<ICategoriaRepository> _categoriaRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ProductoService _sut;

    public BusquedaComandaTests()
    {
        _sut = new ProductoService(
            _productoRepoMock.Object,
            _categoriaRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Categoria CrearCategoriaConId(int id, string nombre, bool activa = true)
    {
        var cat = new Categoria(nombre);
        typeof(Categoria).GetProperty(nameof(Categoria.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(cat, id);
        if (!activa)
        {
            cat.Desactivar();
        }
        return cat;
    }

    private static Producto CrearProductoConId(int id, string nombre, decimal precio, int categoriaId, bool activo = true)
    {
        var prod = new Producto(nombre, precio, categoriaId);
        typeof(Producto).GetProperty(nameof(Producto.Id), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(prod, id);
        if (!activo)
        {
            prod.Desactivar();
        }
        return prod;
    }

    [Fact]
    public async Task BuscarActivosAsync_FiltraSoloProductosActivosDeCategoriasActivas()
    {
        // Arrange
        var catBebidas = CrearCategoriaConId(1, "Bebidas Calientes", activa: true);
        var catInactiva = CrearCategoriaConId(2, "Temporada Invierno", activa: false);

        var p1 = CrearProductoConId(10, "Café Americano", 45m, 1, activo: true);
        var p2 = CrearProductoConId(11, "Café Olla Especial", 50m, 2, activo: true); // Su categoría está inactiva

        _categoriaRepoMock.Setup(r => r.ObtenerActivasAsync())
            .ReturnsAsync(new List<Categoria> { catBebidas });

        _productoRepoMock.Setup(r => r.ObtenerActivosDeCategoriasActivasAsync())
            .ReturnsAsync(new List<Producto> { p1, p2 });

        // Act
        var resultados = await _sut.BuscarActivosAsync("cafe");

        // Assert
        resultados.Should().ContainSingle();
        resultados[0].Nombre.Should().Be("Café Americano");
        resultados[0].CategoriaNombre.Should().Be("Bebidas Calientes");
    }

    [Fact]
    public async Task BuscarActivosAsync_ConsultaLetraPorLetra_PriorizaPrefijoExactoParaSeleccionConEnter()
    {
        // Arrange
        var cat = CrearCategoriaConId(1, "Cafetería");
        var p1 = CrearProductoConId(1, "Descafeinado con Leche", 55m, 1);
        var p2 = CrearProductoConId(2, "Café Espresso", 40m, 1);
        var p3 = CrearProductoConId(3, "Café Americano", 45m, 1);

        _categoriaRepoMock.Setup(r => r.ObtenerActivasAsync())
            .ReturnsAsync(new List<Categoria> { cat });

        _productoRepoMock.Setup(r => r.ObtenerActivosDeCategoriasActivasAsync())
            .ReturnsAsync(new List<Producto> { p1, p2, p3 });

        // Act: Búsqueda progresiva letra por letra ("c" -> "ca" -> "caf" -> "cafe")
        var r1 = await _sut.BuscarActivosAsync("c");
        var r2 = await _sut.BuscarActivosAsync("ca");
        var r3 = await _sut.BuscarActivosAsync("caf");
        var r4 = await _sut.BuscarActivosAsync("cafe");

        // Assert: Los que inician con "Café" aparecen antes que "Descafeinado con Leche" y en orden A-Z
        r4.Should().HaveCount(3);
        r4[0].Nombre.Should().Be("Café Americano");
        r4[1].Nombre.Should().Be("Café Espresso");
        r4[2].Nombre.Should().Be("Descafeinado con Leche");
        r1.Should().HaveCount(3);
        r2.Should().HaveCount(3);
        r3.Should().HaveCount(3);
    }

    [Fact]
    public async Task PruebaDeCaos_BusquedaSinTildesConEnesTokensSalteadosYComodinesBasura_NoRompeYEncuentraExactos()
    {
        // Arrange: Catálogo rico en tildes, eñes, diéresis, inglés y nombres compuestos
        var catBebidas = CrearCategoriaConId(1, "Bebidas y Café");
        var catCocina = CrearCategoriaConId(2, "Desayunos y Sandwiches");

        var catalogoActivo = new List<Producto>
        {
            CrearProductoConId(1, "Café de la Olla (Especial/Navideño)", 55m, 1),
            CrearProductoConId(2, "Baguette de Jalapeño con Piña y Champiñón", 95m, 2),
            CrearProductoConId(3, "Ün Frappé de Moka", 68m, 1),
            CrearProductoConId(4, "Bagel with Salmón y Queso Crema", 110m, 2),
            CrearProductoConId(5, "Ñoquis de Papa con Trufa", 130m, 2)
        };

        _categoriaRepoMock.Setup(r => r.ObtenerActivasAsync())
            .ReturnsAsync(new List<Categoria> { catBebidas, catCocina });

        _productoRepoMock.Setup(r => r.ObtenerActivosDeCategoriasActivasAsync())
            .ReturnsAsync(catalogoActivo);

        // 1. Búsqueda omitiendo conectores ("de la") y sin tildes ni eñe ("cafe olla navideno")
        var resOlla = await _sut.BuscarActivosAsync("   CAFE   olla   navideno  ");
        resOlla.Should().ContainSingle()
            .Which.Nombre.Should().Be("Café de la Olla (Especial/Navideño)");

        // 2. Búsqueda con eñes omitidas ("jalapeno pina champinon")
        var resJalapeno = await _sut.BuscarActivosAsync("jalapeno pina champinon");
        resJalapeno.Should().ContainSingle()
            .Which.Nombre.Should().Be("Baguette de Jalapeño con Piña y Champiñón");

        // 3. Búsqueda con diéresis y acento omitidos ("un frappe")
        var resFrappe = await _sut.BuscarActivosAsync("un frappe");
        resFrappe.Should().ContainSingle()
            .Which.Nombre.Should().Be("Ün Frappé de Moka");

        // 4. Búsqueda combinando palabra del producto y nombre de la categoría ("sandwiches salmon")
        var resPorCategoriaYProducto = await _sut.BuscarActivosAsync("sandwiches salmon");
        resPorCategoriaYProducto.Should().ContainSingle()
            .Which.Nombre.Should().Be("Bagel with Salmón y Queso Crema");

        // 5. Entradas caóticas: comodines SQL, caracteres de control y cadenas vacías
        (await _sut.BuscarActivosAsync("")).Should().BeEmpty();
        (await _sut.BuscarActivosAsync("   \t\r\n  ")).Should().BeEmpty();
        (await _sut.BuscarActivosAsync("%_'\"\\; DROP TABLE Productos;--")).Should().BeEmpty();
    }
}
