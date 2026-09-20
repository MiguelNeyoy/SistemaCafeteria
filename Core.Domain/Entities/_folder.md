# Entities
Entidades del modelo de negocio con identidad única, reglas e invariantes protegidas.

## Catálogo
- `Categoria.cs`: Categorías de productos (Café, Platillos, etc.) con soporte para borrado lógico.
- `Producto.cs`: Productos del menú con precio > 0, categoría y borrado lógico.
- `Extra.cs`: Modificadores o extras del menú (precios >= 0) con borrado lógico.

## Flujo Financiero (Caja / Cuenta)
- `Venta.cs`: Aggregate Root del flujo transaccional. Representa la cuenta abierta o cobrada.
- `VentaItem.cs`: Línea de producto en una venta, con nombre y precio unitario congelados.
- `VentaItemExtra.cs`: Snapshot congelado de extras aplicados a un ítem de venta.
- `Ticket.cs`: Registro y folio del comprobante impreso para el cliente.

## Flujo Operativo (Cocina)
- `Comanda.cs`: Aggregate Root de la orden de preparación enviada a cocina (sin precios).
- `ComandaItem.cs`: Detalle de productos e instrucciones para cocina.
- `ComandaItemExtra.cs`: Extras o instrucciones operativas específicas para cocina.
