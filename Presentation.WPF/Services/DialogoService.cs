using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Presentation.WPF.Views.Alertas;

namespace Presentation.WPF.Services;

public class DialogoService : IDialogoService
{
    public static ObservableCollection<NotificacionItem> ColeccionNotificaciones { get; } = new();

    public bool Mostrar(AlertaProps props)
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            return Application.Current.Dispatcher.Invoke(() => Mostrar(props));
        }

        return AlertaVentana.Mostrar(props, Application.Current?.MainWindow);
    }

    public bool Confirmar(string mensaje, string titulo = "¿Estás seguro?", string textoConfirmar = "Sí", string textoCancelar = "Cancelar")
    {
        return Mostrar(new AlertaProps
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Confirmacion,
            TextoConfirmar = textoConfirmar,
            TextoCancelar = textoCancelar
        });
    }

    public void MostrarError(string mensaje, string titulo = "Error")
    {
        Mostrar(new AlertaProps
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Error,
            TextoConfirmar = "Entendido",
            TextoCancelar = null
        });
    }

    public void MostrarAdvertencia(string mensaje, string titulo = "Advertencia")
    {
        Mostrar(new AlertaProps
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Advertencia,
            TextoConfirmar = "Aceptar",
            TextoCancelar = null
        });
    }

    public void MostrarExito(string mensaje, string titulo = "Operación Exitosa")
    {
        Mostrar(new AlertaProps
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Exito,
            TextoConfirmar = "Aceptar",
            TextoCancelar = null
        });
    }

    public void MostrarInformacion(string mensaje, string titulo = "Información")
    {
        Mostrar(new AlertaProps
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Informacion,
            TextoConfirmar = "Aceptar",
            TextoCancelar = null
        });
    }

    public void MostrarMensaje(string mensaje, string titulo)
    {
        string tLower = titulo.ToLowerInvariant();
        if (tLower.Contains("error") || tLower.Contains("fallo"))
        {
            MostrarError(mensaje, titulo);
        }
        else if (tLower.Contains("exito") || tLower.Contains("éxito") || tLower.Contains("guardad") || tLower.Contains("cancelad"))
        {
            MostrarExito(mensaje, titulo);
        }
        else if (tLower.Contains("vacia") || tLower.Contains("vacía") || tLower.Contains("advertencia") || tLower.Contains("atencion") || tLower.Contains("atención"))
        {
            MostrarAdvertencia(mensaje, titulo);
        }
        else
        {
            MostrarInformacion(mensaje, titulo);
        }
    }

    public void Notificar(NotificacionItem item)
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => Notificar(item));
            return;
        }

        ColeccionNotificaciones.Add(item);

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(Math.Max(2, item.DuracionSegundos))
        };

        timer.Tick += (s, e) =>
        {
            timer.Stop();
            ColeccionNotificaciones.Remove(item);
        };

        timer.Start();
    }

    public static void RemoverNotificacion(NotificacionItem item)
    {
        if (Application.Current?.Dispatcher != null && !Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => RemoverNotificacion(item));
            return;
        }

        ColeccionNotificaciones.Remove(item);
    }

    public void NotificarExito(string mensaje, string titulo = "Éxito", int duracionSegundos = 3)
    {
        Notificar(new NotificacionItem
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Exito,
            DuracionSegundos = duracionSegundos
        });
    }

    public void NotificarError(string mensaje, string titulo = "Error", int duracionSegundos = 4)
    {
        Notificar(new NotificacionItem
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Error,
            DuracionSegundos = duracionSegundos
        });
    }

    public void NotificarAdvertencia(string mensaje, string titulo = "Atención", int duracionSegundos = 3)
    {
        Notificar(new NotificacionItem
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Advertencia,
            DuracionSegundos = duracionSegundos
        });
    }

    public void NotificarInformacion(string mensaje, string titulo = "Aviso", int duracionSegundos = 3)
    {
        Notificar(new NotificacionItem
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = TipoAlerta.Informacion,
            DuracionSegundos = duracionSegundos
        });
    }
}