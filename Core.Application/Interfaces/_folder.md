# Interfaces (Application)
Contratos y puertos para casos de uso, persistencia y hardware.

- `IUnitOfWork.cs`: Contrato para transacciones atómicas.
- `IPrinterService.cs`: Puerto de salida hacia el hardware de impresión física.
- `Services/`: Contratos de casos de uso (`ICategoriaService`, `IProductoService`, `IExtraService`, `IVentaService`, `IComandaService`, `ITicketService`, `ICorteCajaService`, `IPurgaService`, `ISeguridadService`).
- `Repositories/`: Contratos de persistencia para cada entidad del sistema.
