## Decisiones

- Stack: C# + WPF (XAML) + SQLite. No cambiar de motor de base de datos.
- Las ventas se borran mensualmente y por tiempo por petición del cliente (es borrado físico mediante `PurgaService`).
- El Cierre de Caja solo valida cuentas abiertas, imprime el comprobante del día (`DateTime.Today`) y abre el cajón; no borra ventas.

## Trampas conocidas

- Impresora térmica: máximo 42 caracteres por línea en tickets y comandas.
- Al insertar o editar un producto, categoría o extra hay que usar `InsertarOrdenado` / `ReordenarColeccion` o la vista pierde el orden A-Z.
- En comandas divididas por rol, dejar un delay de `800ms` entre tickets consecutivos para permitir que la guillotina física regrese sin atascar el papel.
- EF Core 9 / .NET 10: cualquier cambio en entidades o configuraciones de `AppDbContext` requiere generar migración formal (`dotnet ef migrations add <Nombre> --project Infrastructure.Persistence --startup-project Infrastructure.Persistence`) y mantener ignorado `RelationalEventId.PendingModelChangesWarning` para evitar que `Database.Migrate()` cierre la app al iniciar.
- En clasificadores de texto por palabras clave (como `ClasificadorRolComanda`), las palabras cortas de <= 3 letras (ej. `"te"`) deben compararse por token completo y no con `Contains`, para no coincidir dentro de palabras como `"botella"`, `"pastel"` o `"chocolate"`.
- En WPF, enlazar un `TextBox` con `UpdateSourceTrigger=PropertyChanged` directamente a un `decimal` se traga el punto decimal al escribir (`"45."`). Siempre usar una propiedad `string` intermedia en el ViewModel sanitizada con `FormateadorTextoCatalogo.SanitizarTextoPrecio`.

## Issues resueltos recientemente

- `#7`: Nuevo encabezado del ticket con giro comercial y teléfono (`9a3e0f6`).
- `#2`: Impresión de desglose detallado de tickets del día en Resumen Operativo (`99c95dc`).
- `#3`: Orden alfabético A-Z al crear o editar elementos del catálogo (`c57e6b1`).
- `#11`: Modal táctil de descuentos por porcentaje y monto libre en cobro de cuenta (`6214651`).
- `#12`: Clasificación automática y división de comandas por rol de cocina (`835f947` / `2407534`).
- `#10`: Preformateado automático de nombres (Title Case bilingüe ES/EN) y sanitización de precios en Configuración de Menú.

## Estado actual

- Producción: `v1.1.0`
- En desarrollo: `v1.1.x` / `v1.2.0` (barra buscadora en comanda `#9`, rediseño configuración de menú `#8`).

## Comandos

- Compilar: `dotnet build Sistema-Cafeteria.slnx`
- Pruebas: `dotnet test Sistema-Cafeteria.slnx`
- Release Velopack: `powershell -ExecutionPolicy Bypass -File scripts/build_release.ps1 -Version "<x.y.z>"`
