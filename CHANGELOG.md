# Changelog

All notable changes to PhantomOS will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [0.9.0] - 2026-04-15

### Added
- **Smart Onboarding Wizard**: First-run overlay with three optimization paths: Gaming, Security, and Balanced.
- **Network Analyzer (Gaming Link)**: Real-time latency monitor for Google, Cloudflare, Steam, Epic Games, and Riot Games.
- **Zero Telemetry Shield**: Dual-layer privacy block — `hosts` file DNS blackholing + `DiagTrack` service disabling.
- **Auto-Updater**: Checks GitHub Releases API on startup and shows a non-intrusive banner if an update is available.
- **Extended Deep Cleaner**: Added Discord Cache, Google Chrome Cache, and Microsoft Edge Cache as cleanup targets.
- **Cookie Safety Filter**: Cleaner now excludes session/cookie files to prevent accidental logout.
- **Persistent Settings**: App state (applied tweaks, first-run status) saved in `%APPDATA%\PhantomOS\settings.json`.
- **Activity Log View**: Real-time console of all backend operations visible in the UI.

### Changed
- Bumped version to `0.9.0` across csproj and UpdateService.
- CI/CD pipeline (`build.yml`) updated to skip test step until test project is added.
- Background ping loop uses `CancellationToken` for clean shutdown.

### Fixed
- Redundant closing braces in `MainWindowViewModel.cs` that prevented compilation.
- Missing `using System.Threading.Tasks` directives across multiple services.
- Stale version `0.7.0` hardcoded in `UpdateService` corrected to `0.9.0`.

---

## [0.8.0] - 2026-04-15

### Added
- `NetworkService`, `UpdateService`, `SettingsService` new service layer.
- `ObservableCollection<PingResult>` exposed for data-binding in Privacy tab.
- `ShowWizard`, `IsUpdateAvailable`, `NewVersionMessage` reactive properties in ViewModel.

---

## [0.7.0] - 2026-04-15

### Added
- **Activity Log Tab**: `AppLogs` collection bound to a real-time `Logger.OnLog` event.
- **System Health Gauge**: `TotalHealthScore` computed from Security + Optimization weights.
- **Mica/Acrylic transparency** applied to main window.
- `SettingsService` with JSON persistence.

### Fixed
- `SecurityService` missing `RunAuditAsync` and `CalculateScore` methods.
- `OptimizationService` missing `GetCatalog` and `ApplyBatchAsync` methods.

---

## [0.6.0] - 2026-04-15

### Added
- ISO Modifier Engine via DISM (`IsoService`).
- Deep Cleanup module (`CleanupService`) with `%TEMP%`, Prefetch, and Windows Update cache targets.
- `SmartFix` command to apply all Safe+Recommended tweaks in one click.

---

## [0.5.0] - 2026-04-15

### Added
- `SecurityService` with native UAC and Firewall auditing.
- Seatbelt integration for deep security scans.
- `SeverityToColorConverter` for visual severity badges.

---

## [0.4.0] - 2026-04-15

### Added
- `HardwareService` using WMI for CPU, RAM, GPU, and Disk detection.
- `RecommendationService` for profile-based tweak suggestions.
- `TweakCatalog` with 30+ atomic Windows tweaks across Performance, Privacy, and System.

---

## [0.1.0] - 2026-04-15

### Added
- Initial project scaffold with Avalonia UI, ReactiveUI, and MVVM architecture.
- Professional documentation: README, CONTRIBUTING, SECURITY, PRIVACY_POLICY.
- GitHub Actions CI/CD pipeline (`build.yml`).
- Git Flow branching model (`main`/`develop`).
