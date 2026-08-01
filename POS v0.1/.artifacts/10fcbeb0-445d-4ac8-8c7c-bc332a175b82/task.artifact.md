# Tareas: Integridad de Datos y Optimización

- [ ] **Fase 1: Modelos y Base de Datos**
    - [ ] Actualizar `Category.cs` (propiedad calculada)
    - [ ] Actualizar `Product.cs` (CategoryId, ProviderId)
    - [ ] Actualizar `Purchase.cs` (ProviderId)
    - [ ] Actualizar `FinancialModels.cs` (SourceId)
    - [ ] Configurar índices y FKs en `AppDbContext.cs`
- [ ] **Fase 2: Migración y Lógica de Inicio**
    - [ ] Implementar `ALTER TABLE` en `MainViewModel.cs`
    - [ ] Implementar script de migración de texto a ID
    - [ ] Actualizar carga de datos con `.Include()`
- [ ] **Fase 3: ViewModels y Vistas**
    - [ ] Actualizar `ProductsViewModel` y `CreateProductViewModel`
    - [ ] Actualizar `PurchasesViewModel` y `CreatePurchaseViewModel`
    - [ ] Actualizar `FinancialMovementsViewModel`
    - [ ] Ajustar bindings en XAMLs (ProductsView, SalesHistoryView, etc.)
- [ ] **Fase 4: Verificación**
    - [ ] Limpiar y Recompilar
    - [ ] Verificar integridad de datos post-migración
