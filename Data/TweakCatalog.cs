using System.Collections.Generic;
using Microsoft.Win32;
using PhantomOS.Models;

namespace PhantomOS.Data
{
    public static class TweakCatalog
    {
        public static List<AtomicTweak> Tweaks { get; } = new List<AtomicTweak>
        {
            // --- PRIVACY ---
            new AtomicTweak
            {
                Id = "priv_telemetry",
                Name = "Desactivar Telemetría",
                Description = "Desactiva los informes de errores y el envío de datos de diagnóstico a Microsoft.",
                Category = TweakCategory.Privacy,
                Risk = RiskLevel.Safe,
                WhyActivate = "Mejora la privacidad y reduce el tráfico de red en segundo plano.",
                WhyDeactivate = "Microsoft podría no recibir datos para solucionar fallos específicos en tu hardware.",
                RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\DataCollection",
                RegistryValueName = "AllowTelemetry",
                RegistryValueData = 0,
                RegistryValueKind = RegistryValueKind.DWord
            },
            new AtomicTweak
            {
                Id = "priv_location",
                Name = "Desactivar Servicios de Ubicación",
                Description = "Impide que las aplicaciones accedan a tu ubicación física.",
                Category = TweakCategory.Privacy,
                Risk = RiskLevel.Safe,
                WhyActivate = "Mayor privacidad. Evita que apps rastreen tus movimientos.",
                WhyDeactivate = "Apps como 'Mapas' o el clima no funcionarán automáticamente.",
                RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors",
                RegistryValueName = "DisableLocation",
                RegistryValueData = 1
            },
            new AtomicTweak
            {
                Id = "priv_copilot",
                Name = "Desactivar Windows Copilot",
                Description = "Elimina la integración de IA de Copilot en la barra de tareas y el sistema.",
                Category = TweakCategory.Privacy,
                Risk = RiskLevel.Safe,
                WhyActivate = "Libera recursos y evita el procesamiento de datos por IA en segundo plano.",
                WhyDeactivate = "No podrás usar las funciones de asistencia de IA de Windows.",
                RegistryKey = @"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\WindowsCopilot",
                RegistryValueName = "TurnOffWindowsCopilot",
                RegistryValueData = 1
            },

            // --- GAMING ---
            new AtomicTweak
            {
                Id = "game_priority",
                Name = "Prioridad de GPU para Juegos",
                Description = "Aumenta la prioridad de procesamiento de la GPU para aplicaciones identificadas como juegos.",
                Category = TweakCategory.Gaming,
                Risk = RiskLevel.Moderate,
                WhyActivate = "Reduce micro-stutters y mejora la estabilidad de los FPS.",
                WhyDeactivate = "Otras tareas gráficas en segundo plano (como renderizado de video) podrían verse ralentizadas.",
                RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                RegistryValueName = "GPU Priority",
                RegistryValueData = 8
            },
            new AtomicTweak
            {
                Id = "game_net_throttling",
                Name = "Desactivar Throttling de Red",
                Description = "Elimina el límite de procesamiento de paquetes de red para mejorar el ping.",
                Category = TweakCategory.Gaming,
                Risk = RiskLevel.Safe,
                WhyActivate = "Reduce la latencia (ping) en juegos multijugador.",
                WhyDeactivate = "El tráfico de red muy pesado podría consumir un poco más de CPU.",
                RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                RegistryValueName = "NetworkThrottlingIndex",
                RegistryValueData = unchecked((int)0xFFFFFFFF),
                RegistryValueKind = RegistryValueKind.DWord
            },
            new AtomicTweak
            {
                Id = "game_power_throttling",
                Name = "Desactivar Power Throttling",
                Description = "Impide que Windows limite la potencia de la CPU para aplicaciones en segundo plano.",
                Category = TweakCategory.Gaming,
                Risk = RiskLevel.Moderate,
                WhyActivate = "Asegura que los juegos tengan acceso al 100% de la CPU siempre.",
                WhyDeactivate = "Aumenta ligeramente el consumo de batería en portátiles.",
                RegistryKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling",
                RegistryValueName = "PowerThrottlingOff",
                RegistryValueData = 1
            },

            // --- PERFORMANCE ---
            new AtomicTweak
            {
                Id = "perf_sysmain",
                Name = "Desactivar SysMain (Superfetch)",
                Description = "Detiene el servicio que precarga aplicaciones en RAM.",
                Category = TweakCategory.Performance,
                Risk = RiskLevel.Safe,
                WhyActivate = "Reduce el uso de disco y CPU, especialmente útil si tienes un SSD.",
                WhyDeactivate = "Las apps que usas frecuentemente podrían tardar 1-2 segundos más en abrirse.",
                ServiceName = "SysMain"
            },
            new AtomicTweak
            {
                Id = "perf_sys_resp",
                Name = "Optimizar Responsividad del Sistema",
                Description = "Cambia el porcentaje de recursos reservados para el sistema en favor del usuario.",
                Category = TweakCategory.Performance,
                Risk = RiskLevel.Safe,
                WhyActivate = "El sistema se siente más ágil al abrir ventanas y menús.",
                WhyDeactivate = "Servicios del sistema en segundo plano podrían tardar más en procesar tareas pesadas.",
                RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                RegistryValueName = "SystemResponsiveness",
                RegistryValueData = 0
            },
            new AtomicTweak
            {
                Id = "perf_large_cache",
                Name = "Habilitar Large System Cache",
                Description = "Aumenta el tamaño de la caché de archivos del sistema en RAM.",
                Category = TweakCategory.Performance,
                Risk = RiskLevel.Moderate,
                WhyActivate = "Mejora drásticamente la velocidad de acceso a archivos en servidores o estaciones de trabajo.",
                WhyDeactivate = "Reduce la cantidad de RAM disponible para aplicaciones de usuario pesadas.",
                RegistryKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                RegistryValueName = "LargeSystemCache",
                RegistryValueData = 1
            },

            // --- NETWORK ---
            new AtomicTweak
            {
                Id = "net_tcp_nodelay",
                Name = "Habilitar TCP No Delay (Nagle's Algorithm)",
                Description = "Desactiva el algoritmo de Nagle para enviar paquetes pequeños instantáneamente.",
                Category = TweakCategory.Network,
                Risk = RiskLevel.Moderate,
                WhyActivate = "Reduce drásticamente el lag en juegos y aplicaciones en tiempo real.",
                WhyDeactivate = "Puede aumentar ligeramente el número de paquetes pequeños en la red (insignificante hoy en día).",
                RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSTCPIP\Parameters", // Nota: Esto suele ser por interfaz, lo simplificaremos por ahora o lo extenderemos luego
                RegistryValueName = "TcpNoDelay",
                RegistryValueData = 1
            }
        };

        public static List<TweakProfile> Profiles { get; } = new List<TweakProfile>
        {
            new TweakProfile
            {
                Name = "Gaming & Streaming",
                Description = "Maximiza el rendimiento de la GPU y reduce la latencia de red.",
                Icon = "🎮",
                TweakIds = new List<string> { "game_priority", "game_net_throttling", "game_power_throttling", "perf_sys_resp", "net_tcp_nodelay" }
            },
            new TweakProfile
            {
                Name = "Máxima Privacidad",
                Description = "Desactiva todos los servicios de rastreo y telemetría de Windows.",
                Icon = "🛡️",
                TweakIds = new List<string> { "priv_telemetry", "priv_location", "priv_copilot" }
            }
        };
    }
}
