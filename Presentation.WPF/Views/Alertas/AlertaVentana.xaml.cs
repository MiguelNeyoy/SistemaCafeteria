using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Presentation.WPF.Views.Alertas;

public partial class AlertaVentana : Window
{
    private readonly AlertaProps _props;

    public AlertaVentana(AlertaProps props)
    {
        InitializeComponent();
        _props = props ?? new AlertaProps();

        var mainWindow = Application.Current?.MainWindow;
        if (mainWindow != null && mainWindow != this && mainWindow.IsVisible)
        {
            Owner = mainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        ConfigurarVista();
    }

    private void ConfigurarVista()
    {
        TxtTitulo.Text = _props.Titulo;
        TxtMensaje.Text = _props.Mensaje;

        ConfigurarIconoYColores();
        ConfigurarBotones();
    }

    private void ConfigurarIconoYColores()
    {
        switch (_props.Tipo)
        {
            case TipoAlerta.Exito:
                TxtIcono.Text = "✓";
                TxtIcono.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
                BadgeIcono.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
                break;

            case TipoAlerta.Advertencia:
                TxtIcono.Text = "!";
                TxtIcono.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100"));
                BadgeIcono.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                break;

            case TipoAlerta.Error:
                TxtIcono.Text = "✕";
                TxtIcono.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
                BadgeIcono.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
                break;

            case TipoAlerta.Confirmacion:
                TxtIcono.Text = "?";
                TxtIcono.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E65100"));
                BadgeIcono.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                break;

            case TipoAlerta.Informacion:
            default:
                TxtIcono.Text = "ℹ";
                TxtIcono.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1565C0"));
                BadgeIcono.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                break;
        }
    }

    private void ConfigurarBotones()
    {
        BtnConfirmar.Content = string.IsNullOrWhiteSpace(_props.TextoConfirmar) ? "Aceptar" : _props.TextoConfirmar;

        if (string.IsNullOrWhiteSpace(_props.TextoCancelar))
        {
            BtnCancelar.Visibility = Visibility.Collapsed;
            ColCancelar.Width = new GridLength(0);
            Grid.SetColumnSpan(BtnConfirmar, 2);
        }
        else
        {
            BtnCancelar.Content = _props.TextoCancelar;
            BtnCancelar.Visibility = Visibility.Visible;
            ColCancelar.Width = new GridLength(1, GridUnitType.Star);
            Grid.SetColumnSpan(BtnConfirmar, 1);
        }
    }

    private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
    {
        _props.AlConfirmar?.Invoke();
        DialogResult = true;
        Close();
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        _props.AlCancelar?.Invoke();
        DialogResult = false;
        Close();
    }

    public static bool Mostrar(AlertaProps props, Window? owner = null)
    {
        var ventana = new AlertaVentana(props);
        if (owner != null && owner.IsVisible)
        {
            ventana.Owner = owner;
            ventana.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        return ventana.ShowDialog() == true;
    }
}
