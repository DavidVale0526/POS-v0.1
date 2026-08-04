# Plan de Implementación: Optimización UX/UI (Fase 6)

Este plan aborda los puntos pendientes de la auditoría de Experiencia de Usuario para transformar el POS de una herramienta funcional a una profesional y eficiente.

## Estado Actual (Audit vs. Realidad)

| Punto de Auditoría | Estado Actual | Acción Propuesta |
| :--- | :--- | :--- |
| **Sin confirmación al salir** | 🔴 Pendiente | Implementar diálogo en formularios de creación. |
| **Sin feedback de carga** | 🔴 Pendiente | Añadir indicadores visuales (`ProgressBar`) en procesos pesados. |
| **Sin paginación** | 🔴 Pendiente | Implementar carga diferida o "Ver más" en listas largas. |
| **Sin atajos de teclado** | 🟡 Parcial | Mapear `F-keys` para navegación global y cobro. |
| **Sin modo oscuro/claro** | 🟡 Parcial | Menú lateral ya es oscuro; resto de la app es claro. |
| **Sidebar siempre visible** | 🔴 Pendiente | Implementar botón Hamburguesa para colapsar. |
| **Botón "Switch User"** | 🟢 Hecho | Ya está integrado en la tarjeta de usuario inferior. |
| **Notificaciones Toast** | 🔴 Pendiente | Crear un sistema de mensajes no intrusivos (Snackbar). |

---

## Cambios Propuestos

### 1. Navegación y Sidebar Colapsable
- **Botón Hamburguesa**: Añadir un botón en la parte superior del sidebar para alternar entre ancho completo (220px) y mini (60px, solo iconos).
- **Atajos Globales**:
    - `F1`: Punto de Venta (POS)
    - `F2`: Inventario de Productos
    - `F3`: Reparaciones
    - `F4`: Caja
    - `F8`: Finalizar Cobro (dentro del POS)

### 2. Seguridad de Datos (Confirmaciones)
- **Safe Exit**: Al dar clic en "Cancelar" o "Volver" en un formulario (`CreateProduct`, `CreatePurchase`, `CreateRepair`), el sistema detectará si hay texto ingresado y pedirá confirmación para no perder los datos.

### 3. Feedback Visual (Notificaciones Toast)
- Reemplazar los `MessageBox` de "Éxito" (ej: "Producto guardado", "Venta realizada") por una pequeña barra en la parte inferior derecha que desaparece sola tras 3 segundos. Los errores seguirán siendo modales para llamar la atención.

### 4. Paginación y Carga
- Implementar un sistema de "Cargar 50 más" en el historial de ventas para evitar que la app se bloquee si el historial crece a miles de registros.

---

## Plan de Verificación
- **Atajos**: Probar que las teclas de función cambian de vista desde cualquier parte de la app.
- **Colapso**: Verificar que los iconos del sidebar sean legibles en modo compacto.
- **Safe Exit**: Llenar un formulario a medias, intentar salir y confirmar que el aviso aparece.
