# UseCases (Application Services)
Servicios de aplicación que orquestan los flujos de negocio, reglas de dominio y repositorios.

- `CategoriaService.cs`: Gestión del catálogo de categorías.
- `ProductoService.cs`: Gestión de productos con validación de categorías y borrado lógico.
- `ExtraService.cs`: Gestión de extras y modificadores de cocina.
- `VentaService.cs`: Ciclo de vida completo de cuentas y ventas con datos congelados (snapshot).
- `ComandaService.cs`: Creación, despacho y entrega de órdenes en cocina.
- `TicketService.cs`: Generación de folio único y reimpresión de tickets.
- `CorteCajaService.cs`: Cálculo de corte diario (con fondo inicial) y mensual.
- `PurgaService.cs`: Borrado físico de ventas históricas mayores a 30 días.
- `SeguridadService.cs`: Validación y cambio de PIN de admin con PIN maestro de recuperación.
