using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Core.Domain.Common;

/// <summary>
/// Utilidad de dominio para el preformateado, limpieza y normalización automática
/// de nombres y precios en el catálogo (Categorías, Productos y Extras).
/// </summary>
public static class FormateadorTextoCatalogo
{
    private static readonly HashSet<string> ConectoresMinuscula = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "del", "la", "las", "el", "los", "con", "sin",
        "y", "e", "o", "u", "a", "en", "al", "para", "por",
        "and", "with", "of", "in"
    };

    /// <summary>
    /// Limpia espacios múltiples, saltos de línea o tabuladores y aplica formato de título
    /// respetando conectores en español/inglés, caracteres especiales (ñ, tildes) y delimitadores.
    /// </summary>
    public static string FormatearNombre(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        // 1. Colapsar cualquier secuencia de espacios/saltos/controles en un único espacio
        var palabras = ExtraerPalabrasLimpias(texto);
        if (palabras.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(texto.Length);

        for (int i = 0; i < palabras.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(' ');
            }

            string palabra = palabras[i];
            bool inicioDeSubfrase = i == 0 || EsDelimitadorDeSubfrase(palabras[i - 1]);

            // Si no es el inicio de la frase o subfrase y es un conector, mantener en minúsculas
            if (!inicioDeSubfrase && ConectoresMinuscula.Contains(palabra))
            {
                sb.Append(palabra.ToLowerInvariant());
            }
            else
            {
                CapitalizarPalabraCompuesta(palabra, sb);
            }
        }

        return sb.ToString();
    }

    private static bool EsDelimitadorDeSubfrase(string palabraAnterior)
    {
        if (string.IsNullOrEmpty(palabraAnterior))
        {
            return true;
        }

        char ultimo = palabraAnterior[^1];
        return ultimo is '-' or '/' or ':' or '(' or '[';
    }

    /// <summary>
    /// Sanitiza en tiempo real el texto de entrada de un precio, eliminando símbolos ($ o letras),
    /// normalizando comas a punto decimal, evitando ceros a la izquierda innecesarios (ej. "065" -> "65")
    /// y limitando a máximo 2 decimales.
    /// </summary>
    public static string SanitizarTextoPrecio(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(texto.Length);
        bool tienePunto = false;
        int decimalesCount = 0;

        foreach (char c in texto)
        {
            if (c >= '0' && c <= '9')
            {
                if (tienePunto)
                {
                    if (decimalesCount < 2)
                    {
                        sb.Append(c);
                        decimalesCount++;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else if ((c == '.' || c == ',') && !tienePunto)
            {
                if (sb.Length == 0)
                {
                    sb.Append('0');
                }
                sb.Append('.');
                tienePunto = true;
            }
        }

        // Normalizar ceros a la izquierda en la parte entera (ej. "065" -> "65", pero "0.5" se mantiene)
        string resultado = sb.ToString();
        if (resultado.Length > 1 && resultado[0] == '0' && resultado[1] != '.')
        {
            int idxPrimerNoCero = 0;
            while (idxPrimerNoCero < resultado.Length && resultado[idxPrimerNoCero] == '0')
            {
                idxPrimerNoCero++;
            }

            if (idxPrimerNoCero == resultado.Length)
            {
                return "0";
            }

            if (resultado[idxPrimerNoCero] == '.')
            {
                return "0" + resultado[idxPrimerNoCero..];
            }

            return resultado[idxPrimerNoCero..];
        }

        return resultado;
    }

    /// <summary>
    /// Redondea un precio a 2 decimales estándar para moneda.
    /// </summary>
    public static decimal RedondearPrecio(decimal precio)
    {
        return Math.Round(precio, 2, MidpointRounding.AwayFromZero);
    }

    private static List<string> ExtraerPalabrasLimpias(string texto)
    {
        var palabras = new List<string>();
        var actual = new StringBuilder();

        foreach (char c in texto)
        {
            if (char.IsWhiteSpace(c) || char.IsControl(c))
            {
                if (actual.Length > 0)
                {
                    palabras.Add(actual.ToString());
                    actual.Clear();
                }
            }
            else
            {
                actual.Append(c);
            }
        }

        if (actual.Length > 0)
        {
            palabras.Add(actual.ToString());
        }

        return palabras;
    }

    private static void CapitalizarPalabraCompuesta(string palabra, StringBuilder destino)
    {
        bool capitalizarSiguienteLetra = true;

        foreach (char c in palabra)
        {
            if (char.IsLetter(c))
            {
                if (capitalizarSiguienteLetra)
                {
                    destino.Append(char.ToUpperInvariant(c));
                    capitalizarSiguienteLetra = false;
                }
                else
                {
                    destino.Append(char.ToLowerInvariant(c));
                }
            }
            else
            {
                destino.Append(c);
                // Si hay un delimitador interno como '(', '-', '/', '[', '"' capitalizar la siguiente letra
                if (c is '(' or '-' or '/' or '[' or '"')
                {
                    capitalizarSiguienteLetra = true;
                }
                else if (char.IsDigit(c))
                {
                    // Después de un número (ej. "16oz" o "2x1"), mantener minúscula
                    capitalizarSiguienteLetra = false;
                }
            }
        }
    }
}
