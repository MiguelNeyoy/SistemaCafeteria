# Persistence (Infrastructure)
Implementación del acceso a datos mediante Entity Framework Core y SQLite local.

## Componentes:
- `Data/AppDbContext.cs`: Contexto principal con DbSets y carga de configuraciones Fluent API.
- `Configurations/`: Clases de configuración `IEntityTypeConfiguration<T>` por entidad (claves, índices, relaciones, backing fields, precisión decimal).
- `Repositories/`: Implementaciones concretas de las interfaces de repositorio de `Core.Application`.
- `UnitOfWork.cs`: Implementación del patrón Unit of Work para coordinar transacciones atómicas.
