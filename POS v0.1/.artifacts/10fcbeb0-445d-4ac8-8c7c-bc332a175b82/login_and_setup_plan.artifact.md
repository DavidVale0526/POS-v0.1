# Plan de Implementación: Primer Acceso y Sistema de Login Profesional

Este plan detalla el flujo necesario para que el sistema sea capaz de auto-configurarse la primera vez que un usuario lo abre, eliminando las credenciales hardcodeadas ("admin/admin").

---

## 🚀 Fase 1: Detección de Primer Inicio (Setup Wizard)
- [x] **1.1 Nuevo Modelo de Vista `SetupWizardViewModel`**
- [x] **1.2 Nueva Vista `SetupWizardView.xaml`**
- [x] **1.3 Lógica en `MainViewModel`**

## 🔐 Fase 2: Login Profesional y Flujo de Autenticación
- [x] **2.1 Mejora de `LoginView.xaml`**
- [x] **2.2 Persistencia de Sesión Segura** (Implementado `LogoutCommand` y limpieza de bypass)

## 🛠️ Fase 3: Refactorización de Seguridad (Backend)
- [x] **3.1 Limpieza de `LoadDataFromDatabase`**
- [x] **3.2 Protección de Rutas** (Bloqueo de Hotkeys F1-F7 mediante `ExecuteIfAuthenticated`)

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
