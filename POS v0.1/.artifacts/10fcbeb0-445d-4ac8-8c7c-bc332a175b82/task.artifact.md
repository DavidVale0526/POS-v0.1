# Tareas: Validaciones de Modelo y de Negocio

- [x] **Fase 1: Robustez en Base de Datos**
    - [x] Marcar índices como Únicos en `AppDbContext.cs` (Name, Barcode, Email, Pin)
- [x] **Fase 2: Validaciones de Productos**
    - [x] Validar precios y unicidad en `CreateProductViewModel`
    - [x] Validar Estado y Stock en `PosViewModel.AddProductToCart`
- [x] **Fase 3: Validaciones de Usuarios**
    - [x] Validar Email y PIN único en `CreateUserViewModel`
    - [x] Bloquear auto-eliminación en `UserManagementViewModel`
- [x] **Fase 4: Reglas Financieras y de Borrado**
    - [x] Validar Total >= 0 en POS (Descuentos)
    - [x] Bloquear borrado de Categorías con productos
    - [x] Bloquear borrado de Proveedores con compras
- [x] **Fase 5: Verificación Final**
    - [x] Probar intentos de duplicados y bloqueos de borrado
    - [x] Validar margen de ganancia en productos
