# Walkthrough: Fortalecimiento de Validaciones y Reglas de Negocio

Se han implementado múltiples capas de seguridad para garantizar la integridad de la base de datos y prevenir errores operativos comunes.

## Mejoras Implementadas

### 1. Integridad en Base de Datos (Unique Constraints)
Se han configurado restricciones de **unicidad** a nivel de motor de base de datos (SQLite) para evitar duplicados accidentales que corrompan los reportes:
- **Productos**: No se permiten dos productos con el mismo nombre o código de barras.
- **Usuarios**: Se garantiza que el PIN de acceso y el Email sean únicos por persona.

### 2. Validaciones de Inventario y Ventas
- **Venta Segura**: El POS ahora bloquea automáticamente la adición al carrito de productos sin existencias o marcados como "Inactivos".
- **Rentabilidad**: Al crear un producto, el sistema advierte si el precio de venta es menor al costo, evitando pérdidas accidentales. Además, se impide guardar productos con precio 0.
- **Finanzas**: Se bloqueó la posibilidad de aplicar descuentos que dejen el total de la venta en negativo.

### 3. Seguridad de Usuarios
- **Autenticación Única**: El sistema ahora rechaza la creación de usuarios con PINes duplicados, eliminando el riesgo de suplantación de identidad.
- **Protección de Cuenta**: Se impide que el administrador actual se elimine a sí mismo del sistema.

### 4. Protección contra Datos Huérfanos
Se implementaron bloqueos preventivos de borrado para mantener la consistencia histórica:
- **Categorías**: No se pueden eliminar si tienen productos vinculados.
- **Proveedores**: No se pueden eliminar si tienen compras registradas.

## Guía de Verificación

1. **Prueba de Duplicado**: Intenta crear un usuario con un PIN que ya exista. El sistema mostrará un error.
2. **Venta sin Stock**: Busca un producto con stock 0 e intenta agregarlo al carrito. Deberías recibir un aviso de "Sin Stock".
3. **Control de Borrado**: Ve a la lista de categorías e intenta borrar una que sepas que tiene productos. El sistema bloqueará la acción indicando la razón.
4. **Validación de Margen**: Intenta bajar el precio de venta de un producto por debajo de su costo de compra y observa el mensaje de advertencia.
