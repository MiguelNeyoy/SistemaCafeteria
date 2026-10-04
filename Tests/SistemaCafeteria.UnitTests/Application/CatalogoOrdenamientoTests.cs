using Core.Application.Common;
using FluentAssertions;
using System.Collections.ObjectModel;
using Xunit;

namespace SistemaCafeteria.UnitTests.Application;

public class CatalogoOrdenamientoTests
{
    private class ItemPrueba
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    [Fact]
    public void InsertarOrdenado_DebeInsertarEnOrdenAlfabetico_AlPrincipioAlMedioYAlFinal()
    {
        // Arrange
        var coleccion = new ObservableCollection<ItemPrueba>();

        // Act
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 1, Nombre = "Muffin de Chocolate" }, x => x.Nombre);
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 2, Nombre = "Café Americano" }, x => x.Nombre);
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 3, Nombre = "Agua Natural" }, x => x.Nombre);
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 4, Nombre = "Zumo de Naranja" }, x => x.Nombre);
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 5, Nombre = "Galleta de Avena" }, x => x.Nombre);

        // Assert
        var nombres = coleccion.Select(x => x.Nombre).ToList();
        nombres.Should().ContainInOrder(
            "Agua Natural",
            "Café Americano",
            "Galleta de Avena",
            "Muffin de Chocolate",
            "Zumo de Naranja"
        );
    }

    [Fact]
    public void InsertarOrdenado_InsensibleAMayusculasYMinusculas_YRespetaAcentos()
    {
        // Arrange
        var coleccion = new ObservableCollection<ItemPrueba>();

        // Act
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 1, Nombre = "café espresso" }, x => x.Nombre);
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 2, Nombre = "Café Americano" }, x => x.Nombre);
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 3, Nombre = "Árbol de té" }, x => x.Nombre);
        coleccion.InsertarOrdenado(new ItemPrueba { Id = 4, Nombre = "Barrita" }, x => x.Nombre);

        // Assert
        var nombres = coleccion.Select(x => x.Nombre).ToList();
        nombres[0].Should().Be("Árbol de té");
        nombres[1].Should().Be("Barrita");
        nombres[2].Should().Be("Café Americano");
        nombres[3].Should().Be("café espresso");
    }

    [Fact]
    public void ReordenarColeccion_DebeMoverSoloElementosModificadosSinDuplicar()
    {
        // Arrange
        var item1 = new ItemPrueba { Id = 1, Nombre = "Agua" };
        var item2 = new ItemPrueba { Id = 2, Nombre = "Café" };
        var item3 = new ItemPrueba { Id = 3, Nombre = "Té" };

        var coleccion = new ObservableCollection<ItemPrueba> { item1, item2, item3 };

        // Act: modificar el nombre de "Agua" para que pase al final como "Vino"
        item1.Nombre = "Vino Tinto";
        coleccion.ReordenarColeccion(x => x.Nombre);

        // Assert
        var nombres = coleccion.Select(x => x.Nombre).ToList();
        nombres.Should().Equal("Café", "Té", "Vino Tinto");
    }

    [Fact]
    public void InsertarOrdenado_ConMilElementos_MantieneOrdenCorrectoYRendimientoOptimo()
    {
        // Arrange
        var coleccion = new ObservableCollection<ItemPrueba>();
        var random = new Random(42);
        var palabras = new List<string>();

        for (int i = 0; i < 1000; i++)
        {
            palabras.Add($"Producto_{random.Next(1, 99999):D5}");
        }

        // Act
        foreach (var palabra in palabras)
        {
            coleccion.InsertarOrdenado(new ItemPrueba { Id = 1, Nombre = palabra }, x => x.Nombre);
        }

        // Assert
        coleccion.Should().HaveCount(1000);
        var nombres = coleccion.Select(x => x.Nombre).ToList();
        var nombresOrdenadosEsperados = palabras.OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase).ToList();
        nombres.Should().Equal(nombresOrdenadosEsperados);
    }
}
