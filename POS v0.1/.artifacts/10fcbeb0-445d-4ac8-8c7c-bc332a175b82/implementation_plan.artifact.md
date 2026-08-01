# Plan de Implementación: Validaciones de Modelo y de Negocio

Este plan aborda las faltas de validación en el modelo de datos y las reglas de negocio críticas identificadas en la auditoría, garantizando la integridad de los datos y previniendo estados inconsistentes.

## User Review Required

> [!WARNING]
> **Cambios en Reglas de Venta**: A partir de estos cambios, no se permitirá agregar productos sin stock o inactivos al carrito.
> **Restricciones de Borrado**: No se podrán eliminar categorías o proveedores que tengan productos o compras asociadas (se deberá reasignar o eliminar los elementos vinculados primero).
> **Unicidad de PIN**: Se bloqueará la creación de usuarios con PINes duplicados para evitar suplantaciones accidentales.

## Proposed Changes

### 1. Robustez del Modelo de Datos (DB level)

#### [MODIFY] [AppDbContext.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/Data/AppDbContext.cs)
- Marcar los índices de `Product.Name`, `Product.Barcode`, `User.Email` y `User.Pin` como **Unique**.

---

### 2. Validaciones de Creación (ViewModels)

#### [MODIFY] [ProductViewModels.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/ViewModels/ProductViewModels.cs)
- En `SaveCommand`:
  - Validar que `SellingPrice > 0`.
  - Advertir o bloquear si `SellingPrice < PurchasePrice` (prevenir pérdida).
  - Verificar unicidad de Nombre y Barcode antes de guardar (evitar excepción de DB).

#### [MODIFY] [UserViewModels.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/ViewModels/UserViewModels.cs)
- En `Save`:
  - Validar formato de Email.
  - Verificar que el PIN no esté en uso por otro usuario.
- En `DeleteUserCommand`:
  - Impedir que un usuario se elimine a sí mismo (usuario logueado).

---

### 3. Reglas de Negocio en el POS

#### [MODIFY] [MainViewModel.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/ViewModels/MainViewModel.cs) (PosViewModel)
- En `AddProductToCart`:
  - Validar que el producto esté en estado "Activo".
  - Validar que el stock sea `> 0`.
- En el setter de `Discount`:
  - Asegurar que el descuento (monto o %) no resulte en un `Total < 0`.

#### [MODIFY] [CashControlViewModel.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/ViewModels/CashControlViewModel.cs)
- Validar un mínimo operativo en `OpeningBalance` (ej: no permitir abrir caja con montos negativos o absurdos si la lógica del negocio lo requiere).

---

### 4. Integridad Referencial en Borrados

#### [MODIFY] [CategoryViewModels.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/ViewModels/CategoryViewModels.cs)
- En el comando de eliminar: Bloquear si la categoría tiene productos vinculados (`ProductCount > 0`).

#### [MODIFY] [ProviderViewModels.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/ViewModels/ProviderViewModels.cs)
- En el comando de eliminar: Bloquear si el proveedor tiene compras registradas en el sistema.

## Verification Plan

### Manual Verification
- **Unicidad**: Intentar crear dos productos con el mismo nombre y verificar el mensaje de error.
- **Stock 0**: Intentar vender un producto con stock 0 desde el buscador del POS.
- **Auto-eliminación**: Intentar eliminar el usuario con el que se inició sesión.
- **Margen de ganancia**: Intentar guardar un producto con precio de venta menor al de compra y observar la validación.
- **Borrado con dependencias**: Intentar borrar la categoría "Celulares" si tiene productos y verificar que se impida.
