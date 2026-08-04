# Mega Plan de Implementación Final - POS v0.1

Este documento detalla la hoja de ruta técnica para llevar el sistema POS del 92% al 100% de madurez comercial. El objetivo es garantizar integridad contable, trazabilidad total de inventario y una experiencia de usuario de nivel empresarial.

---

## 🟢 Fase 1: Integridad Financiera (El Dinero)
**Objetivo:** Separar los ingresos por método de pago para un cuadre de caja exacto.
- **Desglose de Caja:** Modificar la entidad `CashSession` para rastrear `CashSales`, `CardSales` y `TransferSales` por separado.
- **Pagos Mixtos:** Implementar en el POS la capacidad de definir montos exactos (Ej: C$100 efectivo + C$50 tarjeta).
- **Reporte de Métodos:** Mostrar en el cierre de caja cuánto se debe entregar en físico y cuánto quedó en cuentas bancarias.

## 🔵 Fase 2: Trazabilidad Total (El Inventario/Kardex)
**Objetivo:** Registro histórico de cada unidad que entra o sale.
- **Kardex de Stock:** Crear tabla `StockMovement` (Fecha, Producto, Cantidad, Tipo, Usuario, Motivo).
- **Ajustes Manuales:** Añadir una ventana para que el administrador pueda corregir stock con una justificación (Ej: Daño, Robo, Regalo).
- **Auditoría:** Vista histórica de movimientos por producto.

## 🟡 Fase 3: Servicios Profesionales (Reparaciones)
**Objetivo:** Automatizar la recepción y el consumo de repuestos.
- **Ticket de Recepción:** Formato de impresión para el cliente con el diagnóstico inicial y condiciones del servicio.
- **Consumo de Inventario:** Permitir que una reparación "consuma" productos del inventario (Ej: Una pantalla nueva) descontándolos automáticamente.

## 🔴 Fase 4: Inteligencia de Negocio (Reportes Visuales)
**Objetivo:** Facilitar la toma de decisiones gerenciales.
- **Gráficos WPF:** Implementar gráficas de barras para ventas semanales y gráficos de pastel para categorías más vendidas.
- **Exportación PDF:** Generar reportes financieros en PDF formal con el logo de la empresa, listos para enviar por correo.

## 🟣 Fase 5: Resiliencia y Seguridad (Estabilidad)
**Objetivo:** Cero pérdida de datos y diagnóstico rápido.
- **Logger Global:** Sistema centralizado que guarda cualquier error del sistema en un archivo `logs/error_log.txt`.
- **Auto-Backups:** Opción para que el sistema realice una copia de la base de datos automáticamente al cerrar la caja.

---

## Estrategia de Ejecución
Se recomienda seguir el orden de las fases para asegurar primero la base contable antes de avanzar a la parte estética o de reportes.
