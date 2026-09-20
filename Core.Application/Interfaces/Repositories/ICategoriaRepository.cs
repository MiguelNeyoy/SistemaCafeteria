using Core.Domain.Entities;

namespace Core.Application.Interfaces.Repositories;

/// <summary>
/// Contrato de persistencia para Categorias.
/// </summary>
public interface ICategoriaRepository
{
    Task<Categoria?> ObtenerPorIdAsync(int id);
    Task<List<Categoria>> ObtenerTodasAsync();
    Task<List<Categoria>> ObtenerActivasAsync();
    Task AgregarAsync(Categoria categoria);
    void Actualizar(Categoria categoria);
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null);
}
