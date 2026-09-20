using Core.Domain.Entities;
using Core.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace SistemaCafeteria.UnitTests.Domain;

public class ProductoTests
{
    [Fact]
    public void Constructor_ConDatosValidos_DebeCrearProductoActivoYConNombreTrim()
    {
        // Arrange
        string nombreConEspacios = "   Latte Caramelo   ";
        decimal precio = 55.00m;
        int categoriaId = 2;

        // Act
        var producto = new Producto(nombreConEspacios, precio, categoriaId);

        // Assert
        producto.Nombre.Should().Be("Latte Caramelo");
        producto.Precio.Should().Be(55.00m);
        producto.CategoriaId.Should().Be(2);
        producto.Activo.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99.99)]
    public void Constructor_ConPrecioInvalido_LanzaDomainValidationException(decimal precioInvalido)
    {
        // Act & Assert
        var act = () => new Producto("Café Americano", precioInvalido, 1);
        act.Should().Throw<DomainValidationException>()
            .Where(ex => ex.Campo == "Precio");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_ConNombreVacio_LanzaDomainValidationException(string? nombreInvalido)
    {
        // Act & Assert
        var act = () => new Producto(nombreInvalido!, 35.00m, 1);
        act.Should().Throw<DomainValidationException>()
            .Where(ex => ex.Campo == "Nombre");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ConCategoriaInvalida_LanzaDomainValidationException(int categoriaInvalida)
    {
        // Act & Assert
        var act = () => new Producto("Muffin", 30.00m, categoriaInvalida);
        act.Should().Throw<DomainValidationException>()
            .Where(ex => ex.Campo == "CategoriaId");
    }

    [Fact]
    public void ModificarPrecio_ConPrecioValido_ActualizaPrecio()
    {
        // Arrange
        var producto = new Producto("Cheesecake", 60.00m, 3);

        // Act
        producto.ModificarPrecio(65.00m);

        // Assert
        producto.Precio.Should().Be(65.00m);
    }

    [Fact]
    public void ModificarPrecio_ConPrecioMenorOIgualACero_LanzaDomainValidationException()
    {
        // Arrange
        var producto = new Producto("Cheesecake", 60.00m, 3);

        // Act & Assert
        var act = () => producto.ModificarPrecio(0m);
        act.Should().Throw<DomainValidationException>()
            .Where(ex => ex.Campo == "Precio");
    }

    [Fact]
    public void DesactivarYActivar_CambianEstadoCorrectamente()
    {
        // Arrange
        var producto = new Producto("Soda Italiana", 40.00m, 2);
        producto.Activo.Should().BeTrue();

        // Act & Assert Desactivar
        producto.Desactivar();
        producto.Activo.Should().BeFalse();

        // Act & Assert Activar
        producto.Activar();
        producto.Activo.Should().BeTrue();
    }
}
