# 📖 Ejemplo Introductorio: PhantomOS en Acción

> Este documento te guía paso a paso en tu primera sesión con PhantomOS, para que entiendas el flujo real de trabajo.

---

## 🧙 Primer Arranque: El Asistente Inteligente

Cuando abras PhantomOS por primera vez, verás el **Wizard de Configuración Inicial**. Solo tienes que elegir tu perfil:

- **🎮 Gaming** → Enfocado en reducción de latencia, prioridad de CPU y buffers de red.
- **🛡️ Security** → Activará automáticamente el blindaje Zero Telemetry y los tweaks de privacidad.
- **⚖️ Balanced** → Aplica todas las optimizaciones marcadas como "Recomendado". Ideal para uso diario.

> Tu elección se guarda. No volverás a ver este Wizard hasta que pulses "Relanzar Configuración" en el Dashboard.

---

## 📊 Entendiendo el Dashboard

El número grande que ves (ej. `87%`) es tu **Índice de Salud Total**. Se calcula así:

```
Salud Total = 50% (Score Seguridad) + 50% (Tweaks Aplicados / Tweaks Recomendados)
```

Un sistema recién instalado puede partir de `50%`. Después de aplicar tweaks y el Security Fix, puede llegar a `90%+`.

---

## 🚀 Aplicar Optimizaciones

Ve a la pestaña **🚀 Optimización**. Verás una lista de tweaks con casillas:

1. Marca los que quieras aplicar (los **verdes/recomendados** → seguros para la mayoría de usuarios).
2. Pulsa **"Aplicar"**.
3. Los IDs de los tweaks aplicados se guardan en `%APPDATA%\PhantomOS\settings.json` para que la próxima vez la app restaure tu estado.

> **Nivel de Riesgo:**
> - `Safe` → Sin efectos secundarios notables.
> - `Moderate` → Puede desactivar funciones secundarias.
> - `Advanced` → Solo para usuarios avanzados.

---

## 🕵️ Zero Telemetry: Cómo Funciona

Ve a la pestaña **👻 Privacidad**. Pulsa **"Activar Zero"**. PhantomOS hará dos cosas:

**1. Modifica el archivo `hosts`** (requiere Admin):
```
0.0.0.0 telemetry.microsoft.com
0.0.0.0 v10.events.data.microsoft.com
...
```

**2. Desactiva el servicio `DiagTrack`** vía `sc.exe config DiagTrack start= disabled`.

Esto elimina la telemetría a nivel de red y de proceso, sin afectar el rendimiento del sistema.

---

## 🧹 Deep Cleaner: Limpieza Segura

Ve a la pestaña **🧹 Limpieza**. Verás los tamaños detectados por carpeta. Están pre-marcadas las que tienen archivos. Pulsa **"Ejecutar Limpieza"**.

> **¿Es seguro?** Sí. PhantomOS **excluye automáticamente** archivos de cookies, login y sesión de Chrome y Edge para que no se cierren tus cuentas.

---

## 🎮 Analizar Latencia de Red

En la pestaña **👻 Privacidad**, el panel superior muestra la latencia en tiempo real a 5 servidores gaming. Se actualiza cada **15 segundos**. Útil para diagnosticar problemas de conexión antes de iniciar una partida.

---

## 📝 El Log de Actividad

Todo lo que hace PhantomOS queda registrado en:
- **Pestaña "📝 Actividad"**: Muestra los últimos 100 eventos en tiempo real dentro de la app.
- **Archivo `phantom_os.log`**: Guardado junto al `.exe` con timestamps `[yyyy-MM-dd HH:mm:ss]`.

---

### 🔗 Recursos

- [← Volver al README](./README.md)
- [Guía de Contribución](./CONTRIBUTING.md)
- [Changelog](./CHANGELOG.md)
