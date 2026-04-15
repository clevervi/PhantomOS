<div align="center">

# 👻 PhantomOS Industrial

**Suite premium de seguridad, privacidad y optimización para Windows**

[![Build](https://github.com/clevervi/PhantomOS/actions/workflows/build.yml/badge.svg)](https://github.com/clevervi/PhantomOS/actions)
[![Release](https://img.shields.io/github/v/release/clevervi/PhantomOS?color=7B2FBE&label=versión)](https://github.com/clevervi/PhantomOS/releases/latest)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2B-blue)](https://github.com/clevervi/PhantomOS)
[![License](https://img.shields.io/github/license/clevervi/PhantomOS?color=green)](LICENSE)

---

### ⬇️ Descargar ahora

[![Descargar para Windows](https://img.shields.io/badge/⬇_DESCARGAR_PHANTOMOS_v1.0.0-7B2FBE?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/clevervi/PhantomOS/releases/latest)

> **Requiere ejecutar como Administrador.** Solo para Windows 10/11 (64-bit).

</div>

---

## ¿Qué es PhantomOS?

**PhantomOS** no es un "limpiador" genérico. Es una suite industrial de gestión de Windows que reúne en una sola interfaz todo lo que necesitas para tener un sistema seguro, rápido y sin telemetría invasiva.

Diseñado con una arquitectura tipo **MVVM** sobre Avalonia UI y .NET 10, ofrece:
- una **UI premium** con efecto Mica/Acrylic glassmorphism
- servicios desacoplados y totalmente **async**
- **7 módulos especializados** accesibles desde una sidebar de navegación fluida

---

## ✨ Módulos y Funcionalidades

### 🏠 1. Dashboard — Estado del Sistema
El panel de control central de PhantomOS te muestra en tiempo real:
- **Índice de Salud Total** (0–100%): calculado como la combinación ponderada del score de seguridad y el nivel de optimización aplicado.
- **Botón "Relanzar Configuración"**: Reabre el Asistente de Setup desde cualquier momento.
- **Banner de actualización no intrusivo**: Si hay una nueva versión en GitHub, aparece un aviso elegante sin interrumpir tu flujo de trabajo.

---

### 🚀 2. Optimización — Motor de Tweaks Atómicos
Un catálogo de más de **30 tweaks de Windows** organizados por categorías:

| Categoría | Descripción |
|-----------|-------------|
| **Performance** | Prioridad de CPU, GameMode, latencia de disco |
| **Privacy** | Desactivación de Cortana, historial de activities, sugerencias |
| **System** | Optimización de memoria virtual, servicios innecesarios |
| **Network** | Ajuste de buffers TCP/IP, reducción de latencia de red |
| **Gaming** | SystemProfile de juegos, GPU prioridad, input lag |

Cada tweak es **reversible** y muestra su nivel de riesgo (**Safe / Moderate / Advanced**).
El botón **"Aplicar"** ejecuta todos los seleccionados en batch con un solo clic.

---

### 🛡️ 3. Seguridad — Auditoría Profunda
Inspirado en la herramienta militar **Seatbelt**, el módulo de seguridad realiza un análisis profundo del sistema:
- Detección de **UAC desactivado**
- Verificación del estado del **Firewall de Windows**
- Escaneo de **servicios vulnerables activos**
- Identificación de **configuraciones de políticas inseguras**

Cada hallazgo incluye un semáforo de severidad por colores (**Critical → Low**) y un botón **"Smart Fix"** para remediar automáticamente.

---

### 👻 4. Privacidad & Red — Blindaje Total

#### 🎮 Gaming Link Pro Analyzer
Monitor de latencia en **tiempo real** (actualizado cada 15 segundos) hacia los principales servidores de gaming:

| Servidor | Estado |
|----------|--------|
| 🌐 Google DNS | 🟢 Verde < 50ms / 🟡 Amarillo < 100ms / 🔴 Rojo |
| ☁️ Cloudflare | 🟢 Verde < 50ms / 🟡 Amarillo / 🔴 Rojo |
| 🎮 Steam (Valve) | 🟢 Verde < 50ms / 🟡 Amarillo / 🔴 Rojo |
| 🎮 Epic Games | 🟢 Verde < 50ms / 🟡 Amarillo / 🔴 Rojo |
| 🎮 Riot Games | 🟢 Verde < 50ms / 🟡 Amarillo / 🔴 Rojo |

#### 🕵️ Zero Telemetry Shield
Protección de privacidad en **doble capa**:
1. **Capa DNS (hosts):** Bloquea los 7 dominios de telemetría de Microsoft directamente en el archivo `hosts` del sistema.
2. **Capa de Servicio:** Desactiva y mata el servicio `DiagTrack` (Connected User Experiences and Telemetry) vía `sc.exe`.

```
0.0.0.0 telemetry.microsoft.com
0.0.0.0 v10.events.data.microsoft.com
0.0.0.0 watson.telemetry.microsoft.com
... y 4 más
```

---

### 💿 5. ISO Modifier Engine
Modifica imágenes de Windows directamente:
- Selección de archivo **WIM** desde el filesystem.
- Inyección de tweaks de PhantomOS en la imagen base.
- Motor basado en **DISM** para compatibilidad total con Windows 10/11.

---

### 🧹 6. Deep Cleaner Pro
Limpieza quirúrgica de archivos basura en **8 ubicaciones de alto impacto**:

| Ubicación | Protección |
|-----------|-----------|
| `%TEMP%` — Archivos del usuario | — |
| `C:\Windows\Temp` — Sistema | — |
| `C:\Windows\Prefetch` | — |
| `C:\Windows\SoftwareDistribution\Download` | — |
| Caché de Iconos/Miniaturas | — |
| **Cache de Discord** | — |
| **Cache de Google Chrome** | 🔒 Cookies protegidas |
| **Cache de Microsoft Edge** | 🔒 Cookies protegidas |

> El sistema excluye automáticamente archivos de sesión (`cookies`, `login`, `session`) para **no cerrar tus cuentas activas**.

---

### 📝 7. Registro de Actividad
Consola de logs en **tiempo real** de todas las operaciones del sistema, con timestamps y niveles `[INFO]`, `[WARN]`, `[ERROR]`. También se guarda en `phantom_os.log` junto al ejecutable para depuración posterior.

---

### 🧙 Smart Onboarding Wizard
Al abrir PhantomOS por primera vez, el **Asistente Inteligente** configura todo en un clic:

| Perfil | Qué activa |
|--------|-----------|
| 🎮 **Gaming** | Tweaks de Performance + System + Network |
| 🛡️ **Security** | Tweaks de Privacy + Zero Telemetry Shield |
| ⚖️ **Balanced** | Todos los tweaks marcados como "Recomendados" |

Tu elección se guarda en `%APPDATA%\PhantomOS\settings.json`. Puedes relanzar el wizard desde el Dashboard en cualquier momento.

---

## 🛠️ Stack Tecnológico

| Componente | Tecnología |
|------------|-----------|
| Framework | .NET 10 (C#) |
| UI | Avalonia UI 12 + ReactiveUI |
| Diseño | Mica / Acrylic Glassmorphism |
| Persistencia | `System.Text.Json` → `%APPDATA%` |
| Red | `System.Net.NetworkInformation.Ping` |
| Servicios Windows | `ServiceController` + `sc.exe` |
| CI/CD | GitHub Actions |

---

## 🚀 Instalación Rápida

```bash
# 1. Descarga el último release
https://github.com/clevervi/PhantomOS/releases/latest

# 2. Descomprime PhantomOS-vX.X.X-win-x64.zip

# 3. Ejecuta como Administrador
PhantomOS.exe
```

> ⚠️ **Requiere permisos de Administrador** para modificar el Registro, los Servicios del sistema y el archivo `hosts`.

---

## 🤝 Contribuir

PhantomOS acepta contribuciones. El camino más fácil es añadir nuevos **Atomic Tweaks** al catálogo:

1. Fork del repo y rama desde `develop`.
2. Agrega el tweak en `Data/TweakCatalog.cs`.
3. Documenta: Registry key, Risk level, descripción.
4. Pull Request contra `develop`.

Ver la [Guía de Contribución](./CONTRIBUTING.md) y el [Ejemplo Introductorio](./Ejemplo-introductorio.md) para más detalles.

---

## 📋 Changelog

Consulta el historial completo de cambios en [CHANGELOG.md](./CHANGELOG.md).

---

<div align="center">

Hecho con 👻 por **[clevervi](https://github.com/clevervi)**

*PhantomOS se proporciona "tal cual". Recomendamos crear un punto de restauración del sistema antes de aplicar optimizaciones profundas.*

</div>
