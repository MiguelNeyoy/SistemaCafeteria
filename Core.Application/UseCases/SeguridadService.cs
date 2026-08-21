using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.Interfaces.Services;
using Core.Domain.Exceptions;

namespace Core.Application.UseCases;

public class SeguridadService : ISeguridadService
{
    private const string ClavePinAdmin = "PIN_ADMIN";
    private const string PinPorDefecto = "1234";
    private const string PinMaestro = "999999";

    private readonly IConfiguracionRepository _configuracionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SeguridadService(IConfiguracionRepository configuracionRepository, IUnitOfWork unitOfWork)
    {
        _configuracionRepository = configuracionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ValidarPinAsync(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            return false;
        }

        var pinAlmacenado = await _configuracionRepository.ObtenerValorAsync(ClavePinAdmin) ?? PinPorDefecto;
        return pin.Trim() == pinAlmacenado;
    }

    public async Task CambiarPinAsync(string pinActual, string pinNuevo)
    {
        if (string.IsNullOrWhiteSpace(pinNuevo) || pinNuevo.Trim().Length < 4)
        {
            throw new DomainValidationException(nameof(pinNuevo), "El nuevo PIN debe contener al menos 4 caracteres.");
        }

        var esValido = await ValidarPinAsync(pinActual);
        if (!esValido)
        {
            throw new DomainException("El PIN actual es incorrecto.");
        }

        await _configuracionRepository.GuardarValorAsync(ClavePinAdmin, pinNuevo.Trim());
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ResetearPinConMaestroAsync(string pinMaestro)
    {
        if (string.IsNullOrWhiteSpace(pinMaestro) || pinMaestro.Trim() != PinMaestro)
        {
            throw new DomainException("El PIN maestro de recuperación es incorrecto.");
        }

        await _configuracionRepository.GuardarValorAsync(ClavePinAdmin, PinPorDefecto);
        await _unitOfWork.SaveChangesAsync();
    }
}
