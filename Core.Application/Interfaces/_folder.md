# Interfaces (Application)
Contratos y puertos para casos de uso y persistencia.

- `IUnitOfWork.cs`: Contrato para confirmar transacciones atómicas.
- `Repositories/`: Contratos de persistencia para cada entidad de dominio (`ICategoriaRepository`, `IProductoRepository`, `IExtraRepository`, `IVentaRepository`, `IComandaRepository`, `ITicketRepository`).
