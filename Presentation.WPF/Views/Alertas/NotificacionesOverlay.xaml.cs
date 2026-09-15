using System.Windows;
using System.Windows.Controls;
using Presentation.WPF.Services;

namespace Presentation.WPF.Views.Alertas;

public partial class NotificacionesOverlay : UserControl
{
    public NotificacionesOverlay()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            ListaToasts.ItemsSource = DialogoService.ColeccionNotificaciones;
        };
    }

    private void BtnCerrarToast_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is NotificacionItem item)
        {
            DialogoService.RemoverNotificacion(item);
        }
    }
}
