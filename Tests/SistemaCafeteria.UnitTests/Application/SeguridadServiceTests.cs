using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Core.Application.UseCases;
using Core.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace SistemaCafeteria.UnitTests.Application;

public class SeguridadServiceTests
{
    private readonly Mock<IConfiguracionRepository> _configRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SeguridadService _service;

    public SeguridadServiceTests()
    {
        _configRepositoryMock = new Mock<IConfiguracionRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _service = new SeguridadService(_configRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task ValidarPin_ConPinPorDefecto_RetornaTrueCuandoNoHayConfiguracion()
    {
        // Arrange: Repositorio no tiene PIN guardado (retorna null)
        _configRepositoryMock.Setup(r => r.ObtenerValorAsync("PIN_ADMIN"))
            .ReturnsAsync((string?)null);

        // Act
        var esValido = await _service.ValidarPinAsync("1234");

        // Assert: "1234" es el PIN por defecto del sistema
        esValido.Should().BeTrue();
    }

    [Theory]
    [InlineData("9999")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ValidarPin_ConPinIncorrecto_RetornaFalse(string? pinInvalido)
    {
        // Arrange
        _configRepositoryMock.Setup(r => r.ObtenerValorAsync("PIN_ADMIN"))
            .ReturnsAsync("1234");

        // Act
        var esValido = await _service.ValidarPinAsync(pinInvalido!);

        // Assert
        esValido.Should().BeFalse();
    }

    [Fact]
    public async Task CambiarPin_ConPinActualCorrectoYNuevoValido_GuardaNuevoPin()
    {
        // Arrange
        _configRepositoryMock.Setup(r => r.ObtenerValorAsync("PIN_ADMIN"))
            .ReturnsAsync("1234");

        // Act
        await _service.CambiarPinAsync("1234", "5678");

        // Assert
        _configRepositoryMock.Verify(r => r.GuardarValorAsync("PIN_ADMIN", "5678"), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Theory]
    [InlineData("12")]
    [InlineData("123")]
    [InlineData("")]
    public async Task CambiarPin_ConPinNuevoMenorA4Digitos_LanzaDomainValidationException(string pinCorto)
    {
        // Act & Assert
        var act = () => _service.CambiarPinAsync("1234", pinCorto);
        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("*al menos 4 caracteres*");
    }

    [Fact]
    public async Task CambiarPin_ConPinActualIncorrecto_LanzaDomainException()
    {
        // Arrange
        _configRepositoryMock.Setup(r => r.ObtenerValorAsync("PIN_ADMIN"))
            .ReturnsAsync("1234");

        // Act & Assert
        var act = () => _service.CambiarPinAsync("0000", "9876");
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*PIN actual es incorrecto*");
    }

    [Fact]
    public async Task ResetearPinConMaestro_ConPinMaestroCorrecto_RestauraPinPorDefecto()
    {
        // Act: El PIN maestro es "4321"
        await _service.ResetearPinConMaestroAsync("4321");

        // Assert
        _configRepositoryMock.Verify(r => r.GuardarValorAsync("PIN_ADMIN", "1234"), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ResetearPinConMaestro_ConPinMaestroInvalido_LanzaDomainException()
    {
        // Act & Assert
        var act = () => _service.ResetearPinConMaestroAsync("9999");
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*PIN maestro*incorrecto*");
    }
}
