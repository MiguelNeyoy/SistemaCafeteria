using System.Diagnostics;
using Core.Application.Common;
using Core.Application.Dtos.Catalogo;
using FluentAssertions;

namespace SistemaCafeteria.UnitTests.Application;

public class FiltroKanbanCatalogoTests
{
    [Theory]
    [InlineData("Café & Capp", "cafe & capp")]
    [InlineData("1 L Agua de Limón", "1 l agua de limon")]
    [InlineData("Frappés Especiales", "frappes especiales")]
    [InlineData("  Té   Frío  ", "te   frio")]
    [InlineData(null, "")]
    public void NormalizarTexto_DebeEliminarTildesYConvertirAMinusculas(string? entrada, string esperado)
    {
        var resultado = FiltroKanbanCatalogo.NormalizarTexto(entrada);
        resultado.Should().Be(esperado);
    }

    [Fact]
    public void CoincideBusqueda_DebeEncontrarProductosConMultiplesTokensSinImportarTildesNiOrden()
    {
        var producto1 = new ProductoDto
        {
            Id = 1,
            Nombre = "1 L Agua de Limón",
            Precio = 60m,
            CategoriaId = 1,
            CategoriaNombre = "Aguas Frescas",
            Activo = true
        };
        var producto2 = new ProductoDto
        {
            Id = 2,
            Nombre = "1/2 L Agua de Limón",
            Precio = 35m,
            CategoriaId = 1,
            CategoriaNombre = "Aguas Frescas",
            Activo = true
        };
        var producto3 = new ProductoDto
        {
            Id = 3,
            Nombre = "Americano 12oz",
            Precio = 35m,
            CategoriaId = 2,
            CategoriaNombre = "Café & Capp",
            Activo = true
        };

        FiltroKanbanCatalogo.CoincideBusqueda(producto1, "agua limon").Should().BeTrue();
        FiltroKanbanCatalogo.CoincideBusqueda(producto2, "LIMON AGUA").Should().BeTrue();
        FiltroKanbanCatalogo.CoincideBusqueda(producto3, "cafe 12oz").Should().BeTrue();
        FiltroKanbanCatalogo.CoincideBusqueda(producto3, "agua limon").Should().BeFalse();
    }

    [Fact]
    public void FiltrarProductosDeCategoria_DebeRetornarSoloLosDeLaCategoriaOrdenadosAlfabeticamente()
    {
        const int catAguas = 10;
        const int catCafe = 20;

        var productos = new List<ProductoDto>
        {
            new() { Id = 1, Nombre = "Té Helado 1L", Precio = 50m, CategoriaId = catAguas, CategoriaNombre = "Aguas Frescas", Activo = true },
            new() { Id = 2, Nombre = "1 L Agua de Jamaica", Precio = 60m, CategoriaId = catAguas, CategoriaNombre = "Aguas Frescas", Activo = true },
            new() { Id = 3, Nombre = "Capuchino Clásico", Precio = 45m, CategoriaId = catCafe, CategoriaNombre = "Café & Capp", Activo = true },
            new() { Id = 4, Nombre = "1/2 L Agua de Horchata", Precio = 35m, CategoriaId = catAguas, CategoriaNombre = "Aguas Frescas", Activo = true }
        };

        var filtrados = FiltroKanbanCatalogo.FiltrarProductosDeCategoria(productos, catAguas, null);

        filtrados.Should().HaveCount(3);
        filtrados.Select(p => p.Nombre).Should().ContainInOrder(
            "1 L Agua de Jamaica",
            "1/2 L Agua de Horchata",
            "Té Helado 1L");
    }

    [Fact]
    public void ObtenerVentanaColumnas_DebeAjustarIndiceInicioCuandoSeOcultanColumnas()
    {
        var todas = new List<string> { "Aguas Frescas", "Café & Capp", "Cocina", "Frappés", "Fruta" };

        // Si el usuario estaba viendo a partir del índice 3 ("Frappés", "Fruta") y oculta varias dejando solo 3
        var tresActivas = new List<string> { "Aguas Frescas", "Café & Capp", "Cocina" };
        var ventana = FiltroKanbanCatalogo.ObtenerVentanaColumnas(tresActivas, indiceInicio: 3, columnasPorVista: 3, out var indiceAjustado);

        indiceAjustado.Should().Be(0);
        ventana.Should().Equal("Aguas Frescas", "Café & Capp", "Cocina");

        // Con las 5 activas y empezando en índice 1, debe mostrar índices 1, 2, 3
        var ventanaIntermedia = FiltroKanbanCatalogo.ObtenerVentanaColumnas(todas, indiceInicio: 1, columnasPorVista: 3, out var indiceIntermedio);
        indiceIntermedio.Should().Be(1);
        ventanaIntermedia.Should().Equal("Café & Capp", "Cocina", "Frappés");
    }

    [Fact]
    public void PruebaDeEstres_20CategoriasY500Productos_ConRafagaDeBusquedasYPaginacion()
    {
        // Escenario extremo: 20 categorías y 500 productos con acentos, fracciones y precios variados
        var categorias = Enumerable.Range(1, 20)
            .Select(i => new CategoriaDto
            {
                Id = i,
                Nombre = $"Categoría #{i:D2} - Menú Especial",
                Activo = true
            })
            .ToList();

        var productos = new List<ProductoDto>(500);
        var nombresBase = new[]
        {
            "1 L Agua de Limón con Chía",
            "1/2 L Agua de Horchata Especial",
            "Café Capuchino Grande",
            "Frappé de Oreo y Cajeta",
            "Sándwich de Pollo a la Parrilla",
            "Té Verde Matcha Frío",
            "Licuado de Plátano con Avena",
            "Jugo Verde Antigripal",
            "Crepa de Fresas con Crema",
            "Chilaquiles Rojos con Pollo"
        };

        for (var i = 0; i < 500; i++)
        {
            var cat = categorias[i % categorias.Count];
            var nombre = $"{nombresBase[i % nombresBase.Length]} ({i + 1})";
            productos.Add(new ProductoDto
            {
                Id = i + 1,
                Nombre = nombre,
                Precio = 35m + (i % 80),
                CategoriaId = cat.Id,
                CategoriaNombre = cat.Nombre,
                Activo = i % 7 != 0
            });
        }

        var consultasRafaga = new[]
        {
            "",
            "a",
            "ag",
            "agu",
            "agua",
            "agua limon",
            "frappe oreo",
            "sandwich parrilla",
            "inexistente_xyz_999",
            ""
        };

        var sw = Stopwatch.StartNew();

        foreach (var consulta in consultasRafaga)
        {
            foreach (var categoria in categorias)
            {
                var resultadoColumna = FiltroKanbanCatalogo.FiltrarProductosDeCategoria(productos, categoria.Id, consulta);

                // Verificar orden alfabético A-Z estricto sin duplicados
                resultadoColumna.Select(p => p.Id).Should().OnlyHaveUniqueItems();
                resultadoColumna.Select(p => p.Nombre).Should().BeInAscendingOrder(StringComparer.CurrentCultureIgnoreCase);
            }

            // Simular paginación de ventana de 3 columnas a lo largo de las 20 categorías
            for (var idx = 0; idx < categorias.Count; idx += 3)
            {
                var ventana = FiltroKanbanCatalogo.ObtenerVentanaColumnas(categorias, idx, 3, out var idxAjustado);
                ventana.Count.Should().BeLessThanOrEqualTo(3);
                idxAjustado.Should().BeInRange(0, categorias.Count - 1);
            }
        }

        sw.Stop();

        // La ráfaga completa de 10 búsquedas sobre 20 columnas x 500 productos debe ser instantánea (< 250ms en cualquier CPU)
        sw.ElapsedMilliseconds.Should().BeLessThan(250);
    }
}
