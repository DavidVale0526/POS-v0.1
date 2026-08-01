# Plan de Implementación: Integridad de Datos y Optimización

Este plan aborda los hallazgos de la sección 4 de la auditoría, centrados en corregir relaciones débiles basadas en texto, agregar índices para mejorar el rendimiento y automatizar contadores desincronizados.

## User Review Required

> [!IMPORTANT]
> **Migración de Datos**: Al cambiar relaciones de `string` a `int (FK)`, se ejecutará un script de migración automático al iniciar la app para vincular los registros existentes mediante sus nombres.
> **Cambio de Propiedades**: Los nombres de algunas propiedades en los modelos cambiarán (ej. `Category` -> `CategoryId`), lo que requiere actualizar los ViewModels y las vistas XAML asociadas.

## Proposed Changes

### 1. Modelos (Relaciones y Propiedades Calculadas)

#### [MODIFY] [Category.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/Models/Category.cs)
- Eliminar la persistencia de `ProductCount`.
- Cambiar `ProductCount` a una propiedad calculada (usando LINQ o navegación).

#### [MODIFY] [Product.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/Models/Product.cs)
- Reemplazar `string Category` por `int CategoryId` y `Category Category`.
- Reemplazar `string Provider` por `int? ProviderId` y `Provider Provider`.

#### [MODIFY] [Purchase.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/Models/Purchase.cs)
- Reemplazar `string ProviderName` por `int? ProviderId` y `Provider Provider`.

#### [MODIFY] [FinancialModels.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/Models/FinancialModels.cs)
- Reemplazar `string Source` por `int SourceId` y `FinancialSource FinancialSource`.

---

### 2. Acceso a Datos (Índices y Configuración)

#### [MODIFY] [AppDbContext.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/Data/AppDbContext.cs)
- Agregar índices en `OnModelCreating` para:
  - `Product.Barcode`
  - `Product.SKU`
  - `User.Email`
  - `User.Pin`
- Configurar las nuevas relaciones de Foreign Key.

---

### 3. Lógica de Migración y Negocio

#### [MODIFY] [MainViewModel.cs](file:///C:/Users/HP%20Pavilioh%20Gaming/source/repos/POS%20v0.1/POS%20v0.1/ViewModels/MainViewModel.cs)
- Actualizar `ApplySchemaUpdates` para incluir los `ALTER TABLE` necesarios (agregar columnas de ID).
- Implementar script de migración manual para poblar `CategoryId`, `ProviderId`, etc., buscando por nombre en las tablas maestras.
- Actualizar la carga de datos inicial para incluir `.Include(...)` en las consultas de EF.

---

### 4. ViewModels y Vistas (Ajustes de Binding)

#### [MODIFY] Varios ViewModels y XAMLs
- Actualizar las pantallas de creación y edición para que seleccionen objetos (IDs) en lugar de escribir nombres.
- Ajustar bindings de `DataGrid` para navegar por las propiedades de objeto (ej. `Category.Name` en lugar de `Category`).

## Verification Plan

### Automated Tests
- No hay suite de pruebas automáticas actual. Se verificará mediante la integridad de la DB post-migración.

### Manual Verification
- **Validación de Migración**: Verificar que los productos existentes conserven su categoría y proveedor después del cambio de esquema.
- **Rendimiento**: Confirmar que las búsquedas por código de barras en el POS sean instantáneas con el nuevo índice.
- **Contadores**: Verificar que al eliminar un producto, el contador de la categoría se actualice correctamente en la vista de lista (al ser ahora calculado).
- **Consistencia**: Cambiar el nombre de una categoría y verificar que todos los productos asociados sigan vinculados a ella.
