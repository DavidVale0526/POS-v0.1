# Plan de Implementación: Módulo de Reportes e Inteligencia de Negocio

Este documento detalla la lógica de negocio, integración de datos y estructura visual para el módulo de Reportes del sistema POS v0.1.

## 1. Objetivo del Módulo
Consolidar la información de todos los módulos operativos (Ventas, Recargas, Reparaciones y Tesorería) para presentar una visión clara de la rentabilidad, flujo de caja y estado del inventario en periodos específicos.

## 2. Integración de Datos (Fuentes)
Para generar los reportes, el sistema consultará:
- **Ventas POS**: Para ingresos brutos y costo de mercancía vendida (CPV).
- **Recargas**: Para flujo de efectivo y comisiones generadas.
- **Reparaciones**: Para ingresos por servicios entregados y cobrados.
- **Tesorería (Gastos/Ingresos)**: Para egresos operativos y otros ingresos no relacionados con ventas directas.
- **Inventario**: Para valoración de activos y stock actual.

## 3. Lógica de Negocio (Cálculos Críticos)

### A. Ventas Brutas
Suma total de todos los tickets de venta finalizados en el periodo seleccionado.

### B. Utilidad Operativa Real
`Utilidad = (Ventas Brutas - Costo de Productos) + Utilidad de Recargas + Ingresos por Reparaciones`
*Nota: No incluye gastos administrativos ni ingresos extraordinarios en este indicador.*

### C. Flujo Total del Negocio (Ingresos vs Egresos)
- **Ingresos**: Ventas POS + Recargas + Reparaciones Cobradas + Otros Ingresos Extras.
- **Egresos**: Gastos Operativos (registrados en Tesorería) + Costo de Productos Vendidos.

### D. Utilidad Neta Total
`Utilidad Neta = Total Ingresos Brutos - Total Egresos`. Es el rendimiento global real del negocio.

### E. Efectivo en Caja (Real)
Cálculo del dinero físico esperado: `(Saldo Inicial + Ingresos en Efectivo) - (Gastos en Efectivo + Retiros)`.

## 4. Estructura de la Interfaz (UI)

### Filtros Superiores
- **Periodo**: Diario, Semanal, Mensual, Anual, Rango Personalizado.
- **Fecha de Referencia**: Selector de fecha para el inicio de la consulta.

### Navegación por Pestañas (Tabs)
1. **Resumen**: Vista general con tarjetas de indicadores clave y balance de flujo (Imagen de referencia).
2. **Finanzas y Utilidad**: Desglose detallado de márgenes de ganancia por categoría.
3. **Inventario**: Valoración total del stock y reporte de productos con baja rotación.
4. **Ventas y Clientes**: Mejores clientes, productos más vendidos y horas pico.
5. **Servicios**: Rendimiento específico de recargas y taller de reparaciones.

## 5. Componentes Técnicos a Crear

### Modelos (`Models/ReportModels.cs`)
- `BusinessSummary`: Estructura para contener los totales calculados.
- `FlowItem`: Representación de una línea de ingreso o egreso en el desglose.

### ViewModels (`ViewModels/ReportsViewModel.cs`)
- `ReportsViewModel`: Manejará la lógica de agregación de datos y el cambio entre pestañas.
- `ApplyFilters()`: Método central para recalcular todos los indicadores al cambiar fechas.

### Vistas (`Views/`)
- `ReportsView.xaml`: Contenedor principal con el diseño de pestañas.
- `ReportSummaryView.xaml`: La vista detallada en la imagen con las tarjetas de colores y el balance.

## 6. Próximos Pasos Sugeridos
1. Crear los modelos de datos para el resumen.
2. Implementar la lógica de cálculo en el ViewModel (conectando las colecciones existentes).
3. Diseñar la interfaz XAML siguiendo el estilo visual de la imagen (Colores: Azul, Verde, Cyan, Amarillo).
4. Implementar la funcionalidad de "Imprimir Reporte" (PDF/Ticket).
