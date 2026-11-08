using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Presentation.WPF.Views
{
    /// <summary>
    /// Lógica de interacción para Comanda.xaml
    /// </summary>
    public partial class Comanda : UserControl
    {
        public Comanda()
        {
            InitializeComponent();
        }

        private void ModalAbrirCuentaOverlay_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TxtIdentificadorCliente.Focus();
                    TxtIdentificadorCliente.SelectAll();
                }), DispatcherPriority.Input);
            }
        }
    }
}
