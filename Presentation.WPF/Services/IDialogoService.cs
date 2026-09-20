using Presentation.WPF.Views.Alertas;

namespace Presentation.WPF.Services;

public interface IDialogoService
{
    // --- Alertas Modales (Bloqueantes / Confirmación) ---
    bool Mostrar(AlertaProps props);
    bool Confirmar(string mensaje, string titulo = "¿Estás seguro?", string textoConfirmar = "Sí", string textoCancelar = "Cancelar");
    void MostrarError(string mensaje, string titulo = "Error");
    void MostrarAdvertencia(string mensaje, string titulo = "Advertencia");
    void MostrarExito(string mensaje, string titulo = "Operación Exitosa");
    void MostrarInformacion(string mensaje, string titulo = "Información");

    // Compatibilidad con código existente
    void MostrarMensaje(string mensaje, string titulo);

    // --- Notificaciones Flotantes / Toasts (No bloqueantes, desaparecen solas) ---
    void Notificar(NotificacionItem item);
    void NotificarExito(string mensaje, string titulo = "Éxito", int duracionSegundos = 3);
    void NotificarError(string mensaje, string titulo = "Error", int duracionSegundos = 4);
    void NotificarAdvertencia(string mensaje, string titulo = "Atención", int duracionSegundos = 3);
    void NotificarInformacion(string mensaje, string titulo = "Aviso", int duracionSegundos = 3);
}