# Plan de Implementación: Historial de Stock y Costo Promedio Ponderado (CPP)

Este plan detalla la implementación de un sistema de seguimiento de movimientos de inventario y el cálculo automático del Costo Promedio Ponderado para una gestión contable precisa.

## User Review Required

> [!IMPORTANT]
> - **Impacto en Precios Existentes:** Al implementar CPP, el `PurchasePrice` de los productos se actualizará automáticamente con cada compra. Los usuarios ya no deberían editar este campo manualmente a menos que sea una corrección de error.
> - **Usuarios:** El historial de stock requiere un usuario responsable. Asegurarse de que `_main.CurrentUser` siempre esté disponible en los ViewModels relevantes.

## Proposed Changes

### 1. Modelos y Datos

#### [NEW] [StockMovement.cs](file:///C:/Users/HP Pavilioh Gaming/source/repos/POS v0.1/POS v0.1/Models/StockMovement.cs)
Creación de la entidad para registrar cada cambio en el stock.

- `Id`: Identificador único.
- `ProductId`: Referencia al producto.
- `Date`: Fecha y hora del movimiento.
- `Quantity`: Cantidad del movimiento (positivo para entradas, negativo para salidas).
- `Type`: Tipo de movimiento ("Venta", "Compra", "Ajuste Manual", "Devolución").
- `PreviousStock`: Stock antes del movimiento.
- `NewStock`: Stock después del movimiento.
- `User`: Nombre del usuario que realizó la acción.
- `Reason`: Nota opcional sobre el motivo.

#### [MODIFY] [AppDbContext.cs](file:///C:/Users/HP Pavilioh Gaming/source/repos/POS v0.1/POS v0.1/Data/AppDbContext.cs)
- Agregar `DbSet<StockMovement> StockMovements`.
- Configurar la relación en `OnModelCreating` para que `StockMovement` tenga un índice por `ProductId`.

---

### 2. Lógica de Negocio (ViewModels)

#### [MODIFY] [PurchaseViewModels.cs](file:///C:/Users/HP Pavilioh Gaming/source/repos/POS v0.1/POS v0.1/ViewModels/PurchaseViewModels.cs)
Implementación de la lógica CPP y registro de movimiento de compra.

- **CPP:** Antes de actualizar el stock, calcular:
  ```csharp
  decimal totalCostExisting = product.Stock * product.PurchasePrice;
  decimal totalCostNew = item.Quantity * item.UnitPrice;
  int totalStock = product.Stock + item.Quantity;
  product.PurchasePrice = totalStock > 0 ? (totalCostExisting + totalCostNew) / totalStock : item.UnitPrice;
  ```
- **Movimiento:** Crear un `StockMovement` por cada item de la compra.

#### [MODIFY] [MainViewModel.cs](file:///C:/Users/HP Pavilioh Gaming/source/repos/POS v0.1/POS v0.1/ViewModels/MainViewModel.cs)
Registro de movimientos de salida por venta.

- Al finalizar la venta (método `FinalizeSale`), crear un `StockMovement` por cada producto vendido con tipo "Venta".

#### [MODIFY] [ProductViewModels.cs](file:///C:/Users/HP Pavilioh Gaming/source/repos/POS v0.1/POS v0.1/ViewModels/ProductViewModels.cs)
Registro de ajustes manuales.

- En la vista de "Agregar Stock" (Quick Adjust), registrar movimiento tipo "Ajuste Manual".
- En el guardado general (Edición de producto), detectar si el `Stock` cambió manualmente y registrar la diferencia.

#### [MODIFY] [SalesHistoryViewModel.cs](file:///C:/Users/HP Pavilioh Gaming/source/repos/POS v0.1/POS v0.1/ViewModels/SalesHistoryViewModel.cs)
Registro de movimientos por devoluciones.

- Al procesar una devolución/reintegro de stock, registrar movimiento tipo "Devolución".

---

### 3. Interfaz de Usuario (Opcional pero Sugerido)

- **ProductDetailView:** Agregar una pestaña o sección "Historial" que muestre la tabla de `StockMovements` filtrada por el producto actual.

## Verification Plan

### Automated Tests
- No hay suite de tests automatizados detectada, se recomienda realizar pruebas manuales siguiendo el flujo de datos.

### Manual Verification
1. **Prueba de CPP:**
   - Producto A tiene Stock 10, Costo C10 (Total: C100).
   - Comprar 10 unidades a C12 (Total: C120).
   - Verificar que el nuevo `PurchasePrice` del producto sea C11 (Total: C220 / 20 unidades).
2. **Prueba de Historial:**
   - Realizar una venta de 2 unidades.
   - Verificar en la base de datos (o nueva vista) que existe un registro con `Quantity = -2` y `Type = "Venta"`.
   - Realizar un ajuste manual de +5.
   - Verificar registro con `Quantity = 5` y `Type = "Ajuste Manual"`.
3. **Verificación de Usuario:**
   - Asegurar que el nombre del usuario logueado aparezca en el historial.
