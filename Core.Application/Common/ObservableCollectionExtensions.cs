using System.Collections.ObjectModel;

namespace Core.Application.Common;

/// <summary>
/// Extensiones para ObservableCollection que permiten mantener el orden alfabetico A-Z sin reiniciar la coleccion completa.
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Inserta un elemento en una coleccion observable manteniendo el orden alfabetico A-Z sin reiniciar la lista.
    /// </summary>
    public static void InsertarOrdenado<T>(this ObservableCollection<T> coleccion, T elemento, Func<T, string> selectorNombre)
    {
        ArgumentNullException.ThrowIfNull(coleccion);
        ArgumentNullException.ThrowIfNull(elemento);
        ArgumentNullException.ThrowIfNull(selectorNombre);

        string nombre = selectorNombre(elemento)?.Trim() ?? string.Empty;
        int index = 0;
        while (index < coleccion.Count && string.Compare(selectorNombre(coleccion[index])?.Trim() ?? string.Empty, nombre, StringComparison.CurrentCultureIgnoreCase) <= 0)
        {
            index++;
        }
        coleccion.Insert(index, elemento);
    }

    /// <summary>
    /// Reordena una coleccion observable in-situ con Move para reflejar cambios de nombre sin recrear los controles de UI.
    /// </summary>
    public static void ReordenarColeccion<T>(this ObservableCollection<T> coleccion, Func<T, string> selectorNombre)
    {
        ArgumentNullException.ThrowIfNull(coleccion);
        ArgumentNullException.ThrowIfNull(selectorNombre);

        var listaOrdenada = coleccion.OrderBy(item => selectorNombre(item)?.Trim() ?? string.Empty, StringComparer.CurrentCultureIgnoreCase).ToList();
        for (int i = 0; i < listaOrdenada.Count; i++)
        {
            int indiceActual = coleccion.IndexOf(listaOrdenada[i]);
            if (indiceActual != i && indiceActual >= 0)
            {
                coleccion.Move(indiceActual, i);
            }
        }
    }
}
