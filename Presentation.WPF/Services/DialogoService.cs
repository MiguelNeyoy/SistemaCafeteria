using System.Windows;

namespace Presentation.WPF.Services;

public class DialogService : IDialogService
{
    public bool Confirmar(string mensaje, string titulo)
    {
        var resultado = MessageBox.Show(
            mensaje,
            titulo,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return resultado == MessageBoxResult.Yes;
    }
}