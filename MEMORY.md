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
- En SQLite, `LIKE` y `UPPER()`/`LOWER()` solo son insensibles a mayúsculas en ASCII y no ignoran tildes ni `ñ`; en buscadores (`BuscarActivosAsync`) normalizar con `NormalizationForm.FormD` sin `NonSpacingMark`, y en `ComandaViewModel` cargar los extras de `producto.CategoriaId` cuando el producto seleccionado proviene de otra categoría.
- En `VentaService.ObtenerTopProductosMesAsync`, agrupar únicamente por `ProductoId` (tomando el último `ProductoNombre` cronológico) y no por `{ ProductoId, ProductoNombre }`, para que si un producto fue renombrado o preformateado a mitad de mes no se divida en dos entradas distintas del ranking.
- En `Presentation.WPF/Views/ConfiguracionMenuView.xaml`, la clase parcial code-behind se llama históricamente `Presentation.WPF.Views.ConfiguracionMenuViewModel` (hereda de `UserControl`); preservar ese `x:Class` al editar la vista para no romper el `DataTemplate` de `MainWindow.xaml`.

## Issues resueltos recientemente

- `#7`: Nuevo encabezado del ticket con giro comercial y teléfono (`9a3e0f6`).
- `#2`: Impresión de desglose detallado de tickets del día en Resumen Operativo (`99c95dc`).
- `#3`: Orden alfabético A-Z al crear o editar elementos del catálogo (`c57e6b1`).
- `#11`: Modal táctil de descuentos por porcentaje y monto libre en cobro de cuenta (`6214651`).
- `#12`: Clasificación automática y división de comandas por rol de cocina (`835f947` / `2407534`).
- `#10`: Preformateado automático de nombres (Title Case bilingüe ES/EN) y sanitización de precios en Configuración de Menú (`e880d29`).
- `#9`: Barra buscadora reactiva letra por letra en pantalla de Categorías (`MenuView`) con selección por `Enter` o clic que navega a `Comanda` y abre el modal de extras si aplica (`09131a4`).
- `#14`: Aumento del listado Top Más Vendidos del mes a 15 productos en Dashboard consolidando por `ProductoId` y desempate alfabético.
- `#8`: Rediseño de pantalla de Configuración de Menú en tablero Kanban por categorías simplificado para personas mayores, con ocultación de columnas, píldoras de estado y buscador reactivo.

## Estado actual

- Producción: `v1.1.0`
- En desarrollo: `v1.2.0` (catálogo Kanban `#8` completado).

## Comandos

- Compilar: `dotnet build Sistema-Cafeteria.slnx`
- Pruebas: `dotnet test Sistema-Cafeteria.slnx`
- Release Velopack: `powershell -ExecutionPolicy Bypass -File scripts/build_release.ps1 -Version "<x.y.z>"`

## Sistema de Diseno UI (Stitch)

- Proyecto Stitch: `UnaMordida` (ID: `5570853905336234929`).
- Tema: `Roasted Earth POS` (Espresso `#442a22`, Honey Amber `#ffb300`, Parchment `#f5f5dc`, Blanco `#ffffff`).
- Tipografias: `Manrope` (titulos), `Work Sans` (cuerpo), `JetBrains Mono` (precios y numeros).
- Dispositivo y Perfil de Usuario: **Monitor normal de escritorio (mouse y teclado)** utilizado por **personas mayores**. Priorizar interfaces simples, limpias, sin exceso de informacion ni iconos cripticos; textos grandes de alto contraste, botones con etiquetas explicitas en espanol (`+ Agregar Producto`, `Editar`) y flujos directos de 1 clic para encontrar/editar o agregar un producto.
- Catalogo real actual en BD (`unamordida.db`): **11 categorias** (`Aguas Frescas` [22], `Café & Capp` [14], `Cocina` [30], `Frappés` [11], `Fruta` [4], `Jugos` [19], `LATTE` [12], `Licuados` [23], `Postres` [6], `Smoothies` [7], `Té` [1]) y **149 productos** (solo tienen `Nombre`, `Precio`, `CategoriaId` y `Activo`; sin subtitulos ni etiquetas adicionales).
- Referencia visual preferida para Configuracion de Menu (`#8`): Estilo **Kanban de Escritorio Simplificado para Personas Mayores** (`projects/5570853905336234929/screens/5cdf53141eeb49a2a5d81256e82b0aee`, variante de `60ee4b93fe164d33b7e9b7a0c0f68ee7` + panel lateral de `3ac82ba2a68c431289d40a3287e61898`), con boton `Ocultar` en cada columna Kanban para esconder categorias y pildoras superiores con borde punteado/ojo tachado para volver a mostrarlas con un clic.
