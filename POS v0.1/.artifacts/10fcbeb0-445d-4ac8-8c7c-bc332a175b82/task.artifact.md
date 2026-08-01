# Tareas: Integridad de Datos y Optimización

- [x] **Fase 1: Modelos y Base de Datos**
    - [x] Actualizar `Category.cs` (propiedad calculada)
    - [x] Actualizar `Product.cs` (CategoryId, ProviderId)
    - [x] Actualizar `Purchase.cs` (ProviderId)
    - [x] Actualizar `FinancialModels.cs` (SourceId)
    - [x] Configurar índices y FKs en `AppDbContext.cs`
- [x] **Fase 2: Migración y Lógica de Inicio**
    - [x] Implementar `ALTER TABLE` en `MainViewModel.cs`
    - [x] Implementar script de migración de texto a ID
    - [x] Actualizar carga de datos con `.Include()`
- [x] **Fase 3: ViewModels y Vistas**
    - [x] Actualizar `ProductsViewModel` y `CreateProductViewModel`
    - [x] Actualizar `PurchasesViewModel` y `CreatePurchaseViewModel`
    - [x] Actualizar `FinancialMovementsViewModel`
    - [x] Ajustar bindings en XAMLs (ProductsView, CreateProductView, FinancialMovementsView)
- [x] **Fase 4: Verificación**
    - [x] Limpiar y Recompilar
    - [x] Verificar integridad de datos post-migración
