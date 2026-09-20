using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Runtime.InteropServices;

namespace Infrastructure.Hardware.Services;

/// <summary>
/// Proporciona acceso de bajo nivel al Spooler de Windows (winspool.drv)
/// para enviar secuencias de bytes crudos (RAW / ESC-POS) a impresoras térmicas.
/// </summary>
public static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDocName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)]
        public string? pDataType;
    }

    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    /// <summary>
    /// Envía un arreglo de bytes en modo RAW al spooler de la impresora indicada.
    /// Lanza una excepción descriptiva si la impresora no existe o no responde.
    /// </summary>
    public static void EnviarBytes(string nombreImpresora, byte[] bytes, string nombreDocumento = "Comprobante POS")
    {
        if (string.IsNullOrWhiteSpace(nombreImpresora))
        {
            throw new InvalidOperationException("No se ha configurado ninguna impresora térmica. Ve a Configuración Avanzada para seleccionar una.");
        }

        if (bytes == null || bytes.Length == 0)
        {
            return;
        }

        IntPtr hPrinter = IntPtr.Zero;
        var di = new DOCINFOA
        {
            pDocName = nombreDocumento,
            pDataType = "RAW"
        };

        if (!OpenPrinter(nombreImpresora.Normalize(), out hPrinter, IntPtr.Zero))
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException(
                $"No se pudo abrir la impresora térmica '{nombreImpresora}'. Verifique que esté conectada por USB y encendida. (Error Windows: {error})");
        }

        try
        {
            if (!StartDocPrinter(hPrinter, 1, di))
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Error al iniciar el documento de impresión en '{nombreImpresora}'. (Error Windows: {error})");
            }

            try
            {
                if (!StartPagePrinter(hPrinter))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"Error al iniciar la página de impresión en '{nombreImpresora}'. (Error Windows: {error})");
                }

                try
                {
                    IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                    try
                    {
                        Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                        if (!WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out int bytesEscritos) || bytesEscritos != bytes.Length)
                        {
                            int error = Marshal.GetLastWin32Error();
                            throw new InvalidOperationException($"Error al transferir los datos a la impresora '{nombreImpresora}'. (Error Windows: {error})");
                        }
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pUnmanagedBytes);
                    }
                }
                finally
                {
                    EndPagePrinter(hPrinter);
                }
            }
            finally
            {
                EndDocPrinter(hPrinter);
            }
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }

    /// <summary>
    /// Obtiene la lista de nombres de impresoras instaladas y reconocidas por el sistema operativo Windows.
    /// </summary>
    public static List<string> ObtenerImpresorasInstaladas()
    {
        var lista = new List<string>();
        try
        {
            foreach (string? printer in PrinterSettings.InstalledPrinters)
            {
                if (!string.IsNullOrWhiteSpace(printer))
                {
                    lista.Add(printer);
                }
            }
        }
        catch
        {
            // Retorna lista vacía si hay algún problema de permisos de servicio de spooler
        }
        return lista;
    }
}
