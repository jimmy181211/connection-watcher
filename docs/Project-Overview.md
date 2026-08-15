# SocketSight Project Overview

## Contents

- [Background and purpose](#background-and-purpose)
- [Project overview](#project-overview)
- [Core design](#core-design)
- [Project structure](#project-structure)
- [Startup, language, and help center](#startup-language-and-help-center)
- [Build and verification](#build-and-verification)

## Background and purpose

Windows Resource Monitor can show current network activity, but the user must keep it open and watch it. A short connection may disappear before it is noticed, and it is not convenient for keeping a focused record of a selected target.

SocketSight lets users define rules for a remote IP, remote port, or local port. It processes only matching TCP connections and records when they appeared, their status, observed duration, the process reported by Windows, and available application context.

It is not a replacement for Resource Monitor or antivirus software. Its purpose is to make a selected connection easier to observe repeatedly and review later, so the user or a cybersecurity professional can investigate further.

## Project overview

SocketSight is a small, rule-based TCP connection observation tool that runs locally on Windows. After monitoring is enabled, it reads the Windows TCP connection table at the selected interval and processes connections that match enabled rules.

The default interval is one second. Users can choose 0.5–10 seconds in 0.5-second steps. Shorter intervals are more likely to catch brief connections but perform more checks; longer intervals use fewer resources but may miss brief connections.

The app records or alerts only for connections selected by the rules. It does not label unrelated network activity as suspicious. The current version focuses on TCP; UDP would require a different lower-level tracing design and more complex application attribution, so it is outside this version.

## Core design

- **Rules first:** only connections matching enabled rules are processed.
- **One observation per connection:** a continuing connection is not written again every second.
- **Wall-clock ending:** a connection is ended only after it has been absent for two seconds; if it returns during that period, it remains the same observation.
- **Application context is evidence:** process, PID, file, parent-process, and Windows-service information can help an investigation but cannot prove the ultimate cause.
- **View and data are separate:** **Clear display** hides older rows without deleting CSV logs.
- **Local by default:** the app does not read packet contents or upload rules and logs. It contacts GitHub only when the user manually checks for updates or opens the feedback page.

## Project structure

```text
connection-watcher/
├── ConnectionWatcher.sln
├── RELEASE_NOTES.md
├── src/
│   ├── ConnectionWatcher.Core/       # rules, monitoring, state, logs, settings
│   └── ConnectionWatcher.App/        # WinForms UI, languages, tray, startup
├── tests/
│   ├── ConnectionWatcher.Tests/      # core and compatibility tests
│   └── ConnectionWatcher.UiSmoke/    # language, DPI, and layout tests
├── docs/                             # project overviews and user guides
├── learning/                         # developer tutorial and learning material
├── scripts/build-release.ps1         # build, test, package, and release preparation
├── packaging/                        # Inno Setup installer definition
└── Final-Share/                      # final files prepared for users
```

- `ConnectionWatcher.Core` contains rule matching, Windows TCP reading, connection tracking, process context, CSV logging, and settings.
- `ConnectionWatcher.App` contains the UI, rule editor, event details, help center, tray notices, alerts, language switching, and startup screen.
- `tests` protects core behavior and checks different languages and display scales.
- `scripts` builds, tests, publishes the self-contained app, creates the installer, copies current documents, and creates SHA-256 checksums.
- `artifacts` is raw publish output, `dist` is installer output, and `Final-Share` is the final user-facing package. All three can be regenerated.

The user downloads one installer: `SocketSight-Setup-win-x64.exe`. The installed app uses a self-contained, multi-file deployment, so users do not need to install the .NET runtime separately.

## Startup, language, and help center

The installer supports seven languages. The language selected during installation also becomes the SocketSight interface language. During an upgrade, a newly selected language replaces the previous interface language once; rules, settings, and logs are kept.

If startup takes longer than about 0.5 seconds, SocketSight shows a short local startup screen. Its rotating messages are only status text; they do not mean that the app is connecting to the Internet or running an extra scan. The screen closes when the main window is ready.

The Help Center in Settings shows the project overview and user guide in the current interface language. Update checking is manual; the app does not download, install, or run updates automatically.

## Build and verification

Building on Windows requires the .NET 8 SDK and Inno Setup.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Maintainers can run:

```powershell
scripts\build-release.ps1
```

The script builds, tests, publishes, creates the installer, collects current documents, and generates SHA-256 checksums. Recipients can use PowerShell `Get-FileHash` to check the installer.
