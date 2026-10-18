using System.Globalization;
using System.Text;
using Core.Application.Dtos.Catalogo;

namespace Core.Application.Common;

/// <summary>
/// Motor de filtrado, búsqueda insensible a acentos y paginación de columnas
/// para el tablero Kanban de Configuración de Menú.
/// </summary>
public static class FiltroKanbanCatalogo
{
    /// <summary>
    /// Normaliza una cadena eliminando tildes/diacríticos y convirtiendo a minúsculas
    /// para búsquedas amigables (ej. "limon" coincide con "1 L Agua de Limón").
    /// </summary>
    public static string NormalizarTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var descompuesto = texto.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);

        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Evalúa si un producto coincide con el término de búsqueda (soporta múltiples palabras parciales sin tildes).
    /// </summary>
    public static bool CoincideBusqueda(ProductoDto producto, string? terminoBusqueda)
    {
        if (producto is null)
            return false;

        if (string.IsNullOrWhiteSpace(terminoBusqueda))
            return true;

        var consultaNormalizada = NormalizarTexto(terminoBusqueda);
        if (string.IsNullOrEmpty(consultaNormalizada))
            return true;

        var nombreNormalizado = NormalizarTexto(producto.Nombre);
        var categoriaNormalizada = NormalizarTexto(producto.CategoriaNombre);

        var tokens = consultaNormalizada.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            if (!nombreNormalizado.Contains(token, StringComparison.Ordinal) &&
                !categoriaNormalizada.Contains(token, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Filtra y ordena alfabéticamente (A-Z) los productos pertenecientes a una categoría según el texto de búsqueda.
    /// </summary>
    public static List<ProductoDto> FiltrarProductosDeCategoria(
        IEnumerable<ProductoDto> productos,
        int categoriaId,
        string? terminoBusqueda)
    {
        return productos
            .Where(p => p.CategoriaId == categoriaId && CoincideBusqueda(p, terminoBusqueda))
            .OrderBy(p => p.Nombre, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Obtiene la ventana visible de columnas Kanban según el índice de inicio y el tamaño de página.
    /// </summary>
    public static IReadOnlyList<T> ObtenerVentanaColumnas<T>(
        IReadOnlyList<T> columnasActivas,
        int indiceInicio,
        int columnasPorVista,
        out int indiceInicioAjustado)
    {
        if (columnasActivas is null || columnasActivas.Count == 0 || columnasPorVista <= 0)
        {
            indiceInicioAjustado = 0;
            return Array.Empty<T>();
        }

        int maxInicio = Math.Max(0, columnasActivas.Count - columnasPorVista);
        indiceInicioAjustado = Math.Clamp(indiceInicio, 0, maxInicio);

        return columnasActivas
            .Skip(indiceInicioAjustado)
            .Take(columnasPorVista)
            .ToList();
    }
}
