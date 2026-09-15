using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Hardware.Services;

/// <summary>
/// Generador fluido de comandos binarios ESC/POS para impresoras térmicas (58mm y 80mm).
/// Soporta formato de texto, tablas, corte de papel y pulso de cajón de dinero.
/// </summary>
public class EscPosBuilder
{
    private readonly List<byte> _buffer = new();
    private readonly int _anchoCaracteres;
    private readonly Encoding _encoding;

    public EscPosBuilder(string ancho = "58mm")
    {
        // 58mm suele tener 32 caracteres por línea; 80mm suele tener 42 o 48.
        _anchoCaracteres = ancho.Trim().ToLower().Contains("80") ? 42 : 32;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        try
        {
            _encoding = Encoding.GetEncoding(850); // CP850 Multilingual Latin I para acentos y ñ
        }
        catch
        {
            _encoding = Encoding.UTF8;
        }

        // Inicializar impresora
        _buffer.AddRange(new byte[] { 0x1B, 0x40 }); // ESC @
        // Seleccionar tabla de caracteres (CP850: ESC t 2)
        _buffer.AddRange(new byte[] { 0x1B, 0x74, 0x02 });
    }

    public EscPosBuilder AlinearIzquierda()
    {
        _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x00 }); // ESC a 0
        return this;
    }

    public EscPosBuilder AlinearCentro()
    {
        _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x01 }); // ESC a 1
        return this;
    }

    public EscPosBuilder AlinearDerecha()
    {
        _buffer.AddRange(new byte[] { 0x1B, 0x61, 0x02 }); // ESC a 2
        return this;
    }

    public EscPosBuilder Negrita(bool activar)
    {
        _buffer.AddRange(new byte[] { 0x1B, 0x45, (byte)(activar ? 0x01 : 0x00) }); // ESC E n
        return this;
    }

    public EscPosBuilder TamanoNormal()
    {
        _buffer.AddRange(new byte[] { 0x1D, 0x21, 0x00 }); // GS ! 0
        return this;
    }

    public EscPosBuilder TamanoDobleAlto()
    {
        _buffer.AddRange(new byte[] { 0x1D, 0x21, 0x01 }); // GS ! 1
        return this;
    }

    public EscPosBuilder TamanoGrande()
    {
        _buffer.AddRange(new byte[] { 0x1D, 0x21, 0x11 }); // GS ! 17 (Doble alto y ancho)
        return this;
    }

    public EscPosBuilder Texto(string texto)
    {
        if (!string.IsNullOrEmpty(texto))
        {
            _buffer.AddRange(_encoding.GetBytes(texto));
        }
        return this;
    }

    public EscPosBuilder Linea(string texto = "")
    {
        Texto(texto);
        _buffer.AddRange(new byte[] { 0x0A }); // LF
        return this;
    }

    public EscPosBuilder LineaSeparadora(char separador = '-')
    {
        Linea(new string(separador, _anchoCaracteres));
        return this;
    }

    public EscPosBuilder Fila2Columnas(string izq, string der)
    {
        izq ??= string.Empty;
        der ??= string.Empty;

        int espacioDisponible = _anchoCaracteres - der.Length;
        if (espacioDisponible < 0)
        {
            Linea(izq);
            AlinearDerecha().Linea(der).AlinearIzquierda();
            return this;
        }

        if (izq.Length > espacioDisponible)
        {
            izq = izq.Substring(0, Math.Max(0, espacioDisponible - 1));
        }

        string relleno = new string(' ', Math.Max(1, _anchoCaracteres - izq.Length - der.Length));
        Linea(izq + relleno + der);
        return this;
    }

    public EscPosBuilder FilaProducto(int cantidad, string nombre, decimal total)
    {
        string prefijoCant = $"{cantidad}x ";
        string textoTotal = total.ToString("C2");

        int anchoDisponibleNombre = _anchoCaracteres - prefijoCant.Length - textoTotal.Length - 1;
        if (anchoDisponibleNombre < 5)
        {
            Linea($"{prefijoCant}{nombre}");
            AlinearDerecha().Linea(textoTotal).AlinearIzquierda();
            return this;
        }

        string nombreAjustado = nombre.Length > anchoDisponibleNombre 
            ? nombre.Substring(0, anchoDisponibleNombre - 1) + "." 
            : nombre;

        int espacios = _anchoCaracteres - (prefijoCant.Length + nombreAjustado.Length + textoTotal.Length);
        if (espacios < 1) espacios = 1;

        Linea(prefijoCant + nombreAjustado + new string(' ', espacios) + textoTotal);
        return this;
    }

    public EscPosBuilder ItemExtra(string nombreExtra, decimal precio)
    {
        string textoExtra = $" + {nombreExtra}";
        string textoPrecio = precio > 0 ? precio.ToString("C2") : "";

        if (string.IsNullOrEmpty(textoPrecio))
        {
            Linea(textoExtra);
        }
        else
        {
            Fila2Columnas(textoExtra, textoPrecio);
        }
        return this;
    }

    public EscPosBuilder Nota(string nota)
    {
        if (!string.IsNullOrWhiteSpace(nota))
        {
            Linea($" * {nota.Trim()}");
        }
        return this;
    }

    public EscPosBuilder AbrirCajon()
    {
        // ESC p 0 25 250 (Pulso eléctrico al puerto RJ11 del cajón de dinero)
        _buffer.AddRange(new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA });
        return this;
    }

    public EscPosBuilder AlimentarLineas(int lineas = 3)
    {
        for (int i = 0; i < lineas; i++)
        {
            _buffer.Add(0x0A);
        }
        return this;
    }

    public EscPosBuilder CortarPapel()
    {
        AlimentarLineas(3);
        // GS V 66 0 (Corte parcial con avance de papel)
        _buffer.AddRange(new byte[] { 0x1D, 0x56, 0x42, 0x00 });
        return this;
    }

    public byte[] Construir()
    {
        return _buffer.ToArray();
    }
}
