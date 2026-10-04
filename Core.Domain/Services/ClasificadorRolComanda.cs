using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Core.Domain.Enums;

namespace Core.Domain.Services;

/// <summary>
/// Servicio de dominio para clasificar automáticamente los productos en sus roles de preparación.
/// Roles:
/// 1. Cocina (Desayunos, Almuerzos, Snacks, Quesadillas, etc.)
/// 2. Barista (Cafetería, Bebidas Calientes, Espresso, Café, Frappés, etc.)
/// 3. Jugos y Licuados (Jugos, Licuados, Aguas frescas, etc.)
/// 4. General (Postres, smoothies de más, o productos no clasificados)
/// </summary>
public static class ClasificadorRolComanda
{
    private static readonly char[] Delimitadores = [' ', ',', '.', '-', '/', '&', '(', ')', '+', '*', '!', ':', ';', '\t', '\r', '\n'];

    public static RolComanda Clasificar(string? nombreProducto, string? nombreCategoria = null)
    {
        string texto = NormalizarTexto($"{nombreCategoria} {nombreProducto}");
        if (string.IsNullOrWhiteSpace(texto)) return RolComanda.General;

        string[] tokens = texto.Split(Delimitadores, StringSplitOptions.RemoveEmptyEntries);

        // 1. General explícito (Smoothies, Postres, Repostería, Panadería)
        if (ContienePalabraClave(texto, tokens,
            "smoothie", "smoothies", "postre", "postres", "reposteria", "panaderia",
            "pastel", "pasteles", "muffin", "muffins", "galleta", "galletas", "dona", "donas",
            "cheesecake", "brownie", "pay", "nieve", "helado"))
        {
            return RolComanda.General;
        }

        // 2. Barista (Cafetería, Espresso, Lattes, Frappés, Té, Tisanas, Bebidas Calientes)
        if (ContienePalabraClave(texto, tokens,
            "barista", "cafe", "cafeteria", "espresso", "latte", "frappe", "capuccino",
            "cappuccino", "moka", "mocha", "americano", "macchiato", "tisana",
            "infusion", "chocolate caliente", "bebidas calientes", "te"))
        {
            return RolComanda.Barista;
        }

        // 3. Jugos y Licuados (Jugos, Licuados, Frutas, Aguas Frescas)
        if (ContienePalabraClave(texto, tokens,
            "jugo", "jugos", "licuado", "licuados", "batido", "batidos", "extracto",
            "agua fresca", "aguas frescas", "jugo verde", "jugos y licuados"))
        {
            return RolComanda.JugosYLicuados;
        }

        // 4. Cocina (Cocina caliente, desayunos, almuerzos, snacks, comida, etc.)
        if (ContienePalabraClave(texto, tokens,
            "cocina", "desayuno", "almuerzo", "comida", "snack", "quesadilla",
            "huevo", "chilaquiles", "torta", "sandwich", "bagel", "panini",
            "omelet", "burrito", "taco", "platillo", "hot cakes", "hotcakes",
            "waffle", "crepa", "mollete", "hamburguesa", "nachos", "papas",
            "boneless", "alitas", "ensalada", "sopa"))
        {
            return RolComanda.Cocina;
        }

        // 5. Default -> General
        return RolComanda.General;
    }

    public static string ObtenerTituloImpresion(RolComanda rol) => rol switch
    {
        RolComanda.Cocina => "*** COCINA ***",
        RolComanda.Barista => "*** BARISTA ***",
        RolComanda.JugosYLicuados => "*** JUGOS Y LICUADOS ***",
        _ => "*** GENERAL ***"
    };

    private static bool ContienePalabraClave(string texto, string[] tokens, params string[] palabrasClave)
    {
        foreach (var palabra in palabrasClave)
        {
            if (palabra.Contains(' '))
            {
                if (texto.Contains(palabra, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            else if (palabra.Length <= 3)
            {
                // Palabras cortas como "te" exigen coincidencia exacta de token completo
                if (tokens.Any(t => string.Equals(t, palabra, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
            else
            {
                // Palabras mayores a 3 letras permiten prefijos (ej: "jugo" -> "jugos", "snack" -> "snacks")
                if (tokens.Any(t => t.StartsWith(palabra, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }
        }
        return false;
    }

    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        // Quitar acentos y diacríticos
        var normalizedString = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}
