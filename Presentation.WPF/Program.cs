using System;
using Velopack;

namespace Presentation.WPF;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // 1. Hook obligatorio de inicio de Velopack (maneja instalación, atajos y actualizaciones)
        VelopackApp.Build().Run();

        // 2. Inicialización normal de la aplicación WPF
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
