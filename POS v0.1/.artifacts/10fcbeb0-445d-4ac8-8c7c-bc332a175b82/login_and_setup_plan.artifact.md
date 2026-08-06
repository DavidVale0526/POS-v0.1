# Plan de Implementación: Primer Acceso y Sistema de Login Profesional

Este plan detalla el flujo necesario para que el sistema sea capaz de auto-configurarse la primera vez que un usuario lo abre, eliminando las credenciales hardcodeadas ("admin/admin").

---

## 🚀 Fase 1: Detección de Primer Inicio (Setup Wizard)
Actualmente, el sistema crea un administrador por defecto si la tabla está vacía. Cambiaremos esto por una experiencia guiada.

- [ ] **1.1 Nuevo Modelo de Vista `SetupWizardViewModel`**
    - Capturar datos iniciales: Nombre del Negocio, Nombre del Administrador, Email, Password y PIN de 4 dígitos.
- [ ] **1.2 Nueva Vista `SetupWizardView.xaml`**
    - Diseño moderno y minimalista para la bienvenida.
    - Validación de campos obligatorios.
- [ ] **1.3 Lógica en `MainViewModel`**
    - Al iniciar, verificar `if (!Db.Users.Any())`.
    - Si está vacío, establecer `CurrentView = SetupWizardViewModel` y `IsAuthenticated = false`.

## 🔐 Fase 2: Login Profesional y Flujo de Autenticación
Una vez que el usuario existe, el sistema debe solicitar credenciales siempre.

- [ ] **2.1 Mejora de `LoginView.xaml`**
    - Añadir opción de "Ver contraseña".
    - Diseño alineado con la estética actual (Azul/Blanco/Gris).
    - Teclado numérico en pantalla (opcional) para facilitar el ingreso del PIN si se desea entrar rápido.
- [ ] **2.2 Persistencia de Sesión Segura**
    - Eliminar el bypass automático en el constructor de `MainViewModel`.
    - Implementar `LogoutCommand` que limpie `CurrentUser` y regrese a `LoginView`.

## 🛠️ Fase 3: Refactorización de Seguridad (Backend)
- [ ] **3.1 Limpieza de `LoadDataFromDatabase`**
    - Eliminar la creación automática de "admin@admin.com".
    - Dejar que el Setup Wizard sea la única fuente de creación del primer administrador.
- [ ] **3.2 Protección de Rutas**
    - Asegurar que los comandos de navegación (F1-F7) no funcionen si `IsAuthenticated` es falso.

---

## 🏁 Flujo del Usuario (Experiencia Final)

1. **Usuario abre el EXE por primera vez:**
   - El sistema ve que no hay dueños.
   - Aparece: "Bienvenido a POS v0.1. Vamos a configurar tu negocio".
   - Crea su cuenta.
2. **Usuario abre el EXE normalmente:**
   - El sistema ve que ya hay usuarios.
   - Aparece la pantalla de Login directamente.
3. **Usuario cambia de turno:**
   - Usa la función "Cambiar Usuario" (ya existente por PIN), pero ahora con una validación de sesión más robusta.

---
**¿Deseas que procedamos con la creación del Setup Wizard ahora mismo?**
