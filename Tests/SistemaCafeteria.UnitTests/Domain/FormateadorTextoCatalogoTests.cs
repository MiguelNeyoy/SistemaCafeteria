using Core.Domain.Common;
using Core.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace SistemaCafeteria.UnitTests.Domain;

public class FormateadorTextoCatalogoTests
{
    [Theory]
    [InlineData("   café   de   la   olla   ", "Café de la Olla")]
    [InlineData("FRAPPÉ DE MOKA", "Frappé de Moka")]
    [InlineData("jalapeño   con   piña   y   champiñón", "Jalapeño con Piña y Champiñón")]
    [InlineData("Ñoquis DE papa", "Ñoquis de Papa")]
    [InlineData("iced   matcha   latte   with   oat   milk", "Iced Matcha Latte with Oat Milk")]
    [InlineData("MAC   AND   CHEESE", "Mac and Cheese")]
    [InlineData("rebanada de pastel (chocolate/vainilla) - extra grande", "Rebanada de Pastel (Chocolate/Vainilla) - Extra Grande")]
    [InlineData("de la casa", "De la Casa")]
    [InlineData("the best coffee in town", "The Best Coffee in Town")]
    public void FormatearNombre_CasosEstandarEBilingues_AplicaTitleCaseYColapsaEspacios(string entrada, string esperado)
    {
        // Act
        var resultado = FormateadorTextoCatalogo.FormatearNombre(entrada);

        // Assert
        resultado.Should().Be(esperado);
    }

    [Theory]
    [InlineData("   \t\r\n   ", "")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void FormatearNombre_EntradaVaciaOSoloEspacios_RetornaCadenaVacia(string? entrada, string esperado)
    {
        // Act
        var resultado = FormateadorTextoCatalogo.FormatearNombre(entrada);

        // Assert
        resultado.Should().Be(esperado);
    }

    [Theory]
    [InlineData("$ 065.50", "65.50")]
    [InlineData("0065..50abc", "65.50")]
    [InlineData("12,5", "12.5")]
    [InlineData("45.999", "45.99")]
    [InlineData(".5", "0.5")]
    [InlineData("0", "0")]
    [InlineData("000", "0")]
    [InlineData("00.75", "0.75")]
    [InlineData("abc$%", "")]
    public void SanitizarTextoPrecio_EntradasSucias_LimpiaYNormalizaCorrectamente(string entrada, string esperado)
    {
        // Act
        var resultado = FormateadorTextoCatalogo.SanitizarTextoPrecio(entrada);

        // Assert
        resultado.Should().Be(esperado);
    }

    [Fact]
    public void PruebaDeCaos_EntradasExtremasBilinguesCaracteresEspecialesYPreciosCorruptos_NoRompeDominioYNormaliza()
    {
        // 1. Caos de texto con saltos de línea, tabuladores, eñes, diéresis, acentos, inglés y delimitadores anidados
        string entradaCaotica = "  \t\r\n  üN   cAfÉ\t\tde   OLLA   (ESPECIAL/navideño) -  con   PIÑA,  jalapeño\r\ny   mac   AND   cheese   WITH   extra   ñandu  \n ";
        string resultadoTexto = FormateadorTextoCatalogo.FormatearNombre(entradaCaotica);

        resultadoTexto.Should().Be("Ün Café de Olla (Especial/Navideño) - Con Piña, Jalapeño y Mac and Cheese with Extra Ñandu");
        resultadoTexto.Should().NotContainAny("\t", "\r", "\n", "  ");

        // 2. Verificar que las 3 entidades de dominio (Categoria, Producto, Extra) aplican el preformateo automáticamente
        var categoria = new Categoria("   bebidas\t\tfrias   DE   temporada (verano/otoño)  ");
        categoria.Nombre.Should().Be("Bebidas Frias de Temporada (Verano/Otoño)");

        var producto = new Producto("  bagel   WITH   salmón   y   queso   crema  ", 89.956m, 1);
        producto.Nombre.Should().Be("Bagel with Salmón y Queso Crema");
        producto.Precio.Should().Be(89.96m);

        var extra = new Extra("   extra   de   jalapeño / piña   ", 15.555m);
        extra.Nombre.Should().Be("Extra de Jalapeño / Piña");
        extra.Precio.Should().Be(15.56m);

        // 3. Ráfaga masiva de cadenas largas con ruido Unicode y precios basura
        string[] entradasRuido =
        [
            "   \n\t  PANINI   DE   PAVO   Y   QUESO   GOUDA   CON   CHIPOTLE   ",
            "smoothie   of   strawberry   and   banana   (16oz/grande)",
            "Té   chai   con   leche   de   almendra - sin   azúcar",
            "   ÑOQUIS   AL   BURRO   CON   TRUFA   Y   PARMESANO   "
        ];

        foreach (var raw in entradasRuido)
        {
            var prod = new Producto(raw, 55.129m, 1);
            prod.Nombre.Should().NotStartWith(" ");
            prod.Nombre.Should().NotEndWith(" ");
            prod.Nombre.Should().NotContain("  ");
            prod.Precio.Should().Be(55.13m);
        }
    }
}
