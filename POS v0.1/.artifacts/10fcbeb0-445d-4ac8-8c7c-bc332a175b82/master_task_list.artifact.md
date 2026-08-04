# Mega Task List: Ruta al 100% de POS v0.1

Esta lista contiene cada paso técnico necesario para completar las fases del Mega Plan.

---

## 🟢 Fase 1: Integridad Financiera y Métodos de Pago
- [ ] **1.1 Base de Datos y Modelos**
    - [ ] Añadir campos `CashTotal`, `CardTotal`, `TransferTotal` a `CashSession`.
    - [ ] Añadir campos `AmountCash`, `AmountCard`, `AmountTransfer` a `Sale`.
- [ ] **1.2 Lógica del POS (Ventas)**
    - [ ] Crear ventana emergente para "Pago Mixto" que pida montos específicos.
    - [ ] Actualizar `SellCommand` para distribuir el total de la venta en los nuevos campos de `Sale`.
    - [ ] Modificar la actualización de `CashSession` en tiempo real al vender.
- [ ] **1.3 Cierre de Caja**
    - [ ] Actualizar `CashControlView` para mostrar el desglose de lo que debería haber en Efectivo vs Bancos.
    - [ ] Incluir el desglose de métodos en el ticket de reporte impreso.

## 🔵 Fase 2: Kardex de Inventario (Trazabilidad)
- [ ] **2.1 Base de Datos**
    - [ ] Crear entidad `StockMovement` (Id, ProductId, Change, Type, Reason, Date, UserId).
- [ ] **2.2 Registro Automático**
    - [ ] Disparar registro en Kardex al vender desde el POS.
    - [ ] Disparar registro en Kardex al registrar una compra.
    - [ ] Disparar registro en Kardex al anular una venta.
- [ ] **2.3 Gestión de Ajustes**
    - [ ] Crear diálogo de "Ajuste Manual de Inventario" en la vista de productos.
    - [ ] Crear vista de "Historial de Movimientos" para consultar la auditoría de un producto.

## 🟡 Fase 3: Módulo de Reparaciones Avanzado
- [ ] **3.1 Impresión de Tickets**
    - [ ] Implementar `PrintRepairReceptionTicket` en `TicketPrinter.cs`.
    - [ ] Vincular el botón de imprimir en el detalle de reparación.
- [ ] **3.2 Integración con Stock**
    - [ ] Añadir sección de "Repuestos Usados" en el detalle de reparación.
    - [ ] Implementar lógica para descontar stock y sumar al costo de la reparación.

## 🔴 Fase 4: Reportes e Inteligencia Visual
- [ ] **4.1 Visualización de Datos**
    - [ ] Integrar componentes de gráficas (LiveCharts o similar) en `ReportsView`.
    - [ ] Crear gráfica: Ventas de los últimos 7 días.
    - [ ] Crear gráfica: Top 5 Categorías más rentables.
- [ ] **4.2 Exportación Profesional**
    - [ ] Implementar generador de PDF para reportes con diseño formal (A4).

## 🟣 Fase 5: Estabilidad y Seguridad (DevOps)
- [ ] **5.1 Manejo de Errores**
    - [ ] Crear clase `Logger` estática para escritura en disco.
    - [ ] Envolver `App.xaml.cs` en un bloque global para capturar crasheos no controlados.
- [ ] **5.2 Automatización**
    - [ ] Implementar `AutoBackupOnClose` en la configuración global.

---
**Nota:** Utiliza este archivo para marcar el progreso diario. ¡Vamos por ese 100%!
