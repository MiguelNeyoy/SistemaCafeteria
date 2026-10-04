using System.Collections.Generic;
using System.Linq;
using Core.Application.Dtos.Comandas;
using Core.Domain.Enums;
using Core.Domain.Services;
using FluentAssertions;
using Xunit;

namespace SistemaCafeteria.UnitTests.Domain;

public class ClasificadorRolComandaTests
{
    [Theory]
    [InlineData("Cocina", "Desayuno Completo", RolComanda.Cocina)]
    [InlineData("Desayunos", "Huevos al gusto", RolComanda.Cocina)]
    [InlineData("Comida", "Hamburguesa con Papas", RolComanda.Cocina)]
    [InlineData("Almuerzos", "Club Sándwich", RolComanda.Cocina)]
    [InlineData("Sandwiches", "Sandwich de Jamón", RolComanda.Cocina)]
    [InlineData("Snacks", "Nachos con Queso", RolComanda.Cocina)]
    public void Clasificar_CategoriasCocina_RetornaCocina(string categoria, string producto, RolComanda esperado)
    {
        var rol = ClasificadorRolComanda.Clasificar(categoria, producto);
        rol.Should().Be(esperado);
    }

    [Theory]
    [InlineData("Cafetería", "Latte Vainilla", RolComanda.Barista)]
    [InlineData("Café & Espresso", "Americano Doble", RolComanda.Barista)]
    [InlineData("Barista", "Cappuccino", RolComanda.Barista)]
    [InlineData("Bebidas Calientes", "Té Verde", RolComanda.Barista)]
    [InlineData("Té", "Infusión Manzanilla", RolComanda.Barista)]
    public void Clasificar_CategoriasBarista_RetornaBarista(string categoria, string producto, RolComanda esperado)
    {
        var rol = ClasificadorRolComanda.Clasificar(categoria, producto);
        rol.Should().Be(esperado);
    }

    [Theory]
    [InlineData("Jugos", "Jugo Verde", RolComanda.JugosYLicuados)]
    [InlineData("Licuados", "Licuado de Fresa", RolComanda.JugosYLicuados)]
    [InlineData("Jugos y Licuados", "Licuado Plátano Choco", RolComanda.JugosYLicuados)]
    [InlineData("Bebidas Frías", "Jugo de Naranja", RolComanda.JugosYLicuados)]
    public void Clasificar_CategoriasJugosYLicuados_RetornaJugosYLicuados(string categoria, string producto, RolComanda esperado)
    {
        var rol = ClasificadorRolComanda.Clasificar(categoria, producto);
        rol.Should().Be(esperado);
    }

    [Theory]
    [InlineData("Postres", "Rebanada Pastel de Zanahoria", RolComanda.General)]
    [InlineData("Smoothies", "Smoothie de Mango", RolComanda.General)]
    [InlineData("Repostería", "Muffin de Arándanos", RolComanda.General)]
    [InlineData("Otros", "Botella de Agua", RolComanda.General)]
    public void Clasificar_CategoriasGeneral_RetornaGeneral(string categoria, string producto, RolComanda esperado)
    {
        var rol = ClasificadorRolComanda.Clasificar(categoria, producto);
        rol.Should().Be(esperado);
    }

    [Fact]
    public void Clasificar_SinCategoria_ClasificaPorNombreDeProducto()
    {
        ClasificadorRolComanda.Clasificar(null, "Café Latte").Should().Be(RolComanda.Barista);
        ClasificadorRolComanda.Clasificar("", "Licuado de Avena").Should().Be(RolComanda.JugosYLicuados);
        ClasificadorRolComanda.Clasificar(null, "Quesadilla con Carne").Should().Be(RolComanda.Cocina);
        ClasificadorRolComanda.Clasificar(null, "Pastel de Chocolate").Should().Be(RolComanda.General);
    }

    [Fact]
    public void ObtenerTituloImpresion_RetornaFormatoCorrecto()
    {
        ClasificadorRolComanda.ObtenerTituloImpresion(RolComanda.Cocina).Should().Be("*** COCINA ***");
        ClasificadorRolComanda.ObtenerTituloImpresion(RolComanda.Barista).Should().Be("*** BARISTA ***");
        ClasificadorRolComanda.ObtenerTituloImpresion(RolComanda.JugosYLicuados).Should().Be("*** JUGOS Y LICUADOS ***");
        ClasificadorRolComanda.ObtenerTituloImpresion(RolComanda.General).Should().Be("*** GENERAL ***");
    }

    [Fact]
    public void AgruparComanda_DivideEnRolesCorrectamente()
    {
        // Arrange
        var items = new List<ComandaItemResumenDto>
        {
            new() { ProductoNombre = "Huevos Rancheros", Rol = RolComanda.Cocina, Cantidad = 1 },
            new() { ProductoNombre = "Chilaquiles", Rol = RolComanda.Cocina, Cantidad = 1 },
            new() { ProductoNombre = "Cappuccino", Rol = RolComanda.Barista, Cantidad = 2 },
            new() { ProductoNombre = "Licuado de Fresa", Rol = RolComanda.JugosYLicuados, Cantidad = 1 },
            new() { ProductoNombre = "Smoothie de Fresa", Rol = RolComanda.General, Cantidad = 1 }
        };

        // Act
        var grupos = items.GroupBy(i => i.Rol).OrderBy(g => (int)g.Key).ToList();

        // Assert
        grupos.Should().HaveCount(4);
        grupos.Select(g => g.Key).Should().ContainInOrder(
            RolComanda.General,
            RolComanda.Cocina,
            RolComanda.Barista,
            RolComanda.JugosYLicuados);

        grupos.First(g => g.Key == RolComanda.Cocina).Should().HaveCount(2);
        grupos.First(g => g.Key == RolComanda.Barista).Should().HaveCount(1);
        grupos.First(g => g.Key == RolComanda.JugosYLicuados).Should().HaveCount(1);
        grupos.First(g => g.Key == RolComanda.General).Should().HaveCount(1);
    }
}
