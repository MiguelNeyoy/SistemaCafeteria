using Core.Application.Interfaces.Repositories;
using Core.Domain.Entities;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly AppDbContext _context;

    public ConfiguracionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ObtenerValorAsync(string clave)
    {
        var config = await _context.Configuraciones.FindAsync(clave);
        return config?.Valor;
    }

    public async Task GuardarValorAsync(string clave, string valor)
    {
        var config = await _context.Configuraciones.FindAsync(clave);
        if (config == null)
        {
            config = new Configuracion(clave, valor);
            await _context.Configuraciones.AddAsync(config);
        }
        else
        {
            config.ActualizarValor(valor);
            _context.Configuraciones.Update(config);
        }
    }
}
