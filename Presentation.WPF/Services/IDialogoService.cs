namespace Presentation.WPF.Services;

public interface IDialogoService
{
    bool Confirmar(string mensaje, string titulo);
    void MostrarMensaje(string mensaje, string titulo);
}