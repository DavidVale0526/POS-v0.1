# Tareas: Autorizaciones de Administrador (PIN)

- [x] **Fase 1: Seguridad en Historial de Ventas**
    - [x] Implementar validación de PIN de Administrador al anular una venta en `SalesHistoryViewModel`.
- [x] **Fase 2: Control de Descuentos en POS**
    - [x] Definir umbral de "Descuento Grande" (> 15%).
    - [x] Implementar validación de PIN al intentar finalizar una venta con descuento alto en `PosViewModel`.
- [x] **Fase 3: Refuerzo de Mensajes**
    - [x] Asegurar que el diálogo indique claramente que se requiere autorización de supervisor.
- [ ] **Fase 4: Verificación**
    - [ ] Probar anulación con usuario normal (debe pedir PIN).
    - [ ] Probar descuento del 5% (debe permitir).
    - [ ] Probar descuento del 20% (debe pedir PIN).
    - [ ] Probar las mismas acciones siendo Administrador (no debe pedir nada).
