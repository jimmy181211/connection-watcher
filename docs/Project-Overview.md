# TCP Connection Watcher

## Background and purpose

When investigating an unusual network connection, we often need to answer a simple question that can be difficult to confirm in time:

> Has my computer connected to a specific IP address or port? If it has, when did it happen, which process did Windows associate with it, and what application context can be recovered?

Windows Resource Monitor can show current network activity, but the user must open it and keep watching it. A short connection may disappear quickly, and watching the window for a long time is not practical. It also does not automatically alert the user about a selected target or keep a continuous record of it.

TCP Connection Watcher helps solve this problem. After the user selects an IP address or port to watch, the app checks for matching connections in the background. When it finds one, it records the time, addresses, ports, the connection owner reported by Windows, and available file, parent-process, and Windows-service context. It then alerts the user according to their settings.

This tool is not intended to replace Resource Monitor or antivirus software. It helps users monitor selected targets, keep records, and provide information for a later security investigation.

## Project overview

TCP Connection Watcher is a small, rule-based **Windows network connection monitoring tool**. Users can choose the remote IP address, remote port, or local port they care about. When Windows reports a TCP connection that matches an enabled rule, the app records it or alerts the user according to that rule.

Simply put, it helps you watch a specified IP address or port. For example, you can tell it to watch `103.1.40.235:1433`. After monitoring starts, if the computer connects to that target, the app records the connection time, active or ended status, observed duration, Windows-reported owner, PID, and any available application context. Depending on the settings, it can **log silently, show a tray notice, or display a pop-up alert.**

The default check interval is one second. Users can choose 0.5–10 seconds in 0.5-second steps. A shorter interval is more likely to catch brief connections; a longer interval uses fewer resources but may miss them.

The app only tells you, “A connection you asked me to watch has appeared.” It does not label other network connections as suspicious, and one connection alone cannot prove that the computer has a virus. The saved information can be shared with a cybersecurity team for further investigation.

## Project structure

```text
connection-watcher/
├── ConnectionWatcher.sln
├── RELEASE_NOTES.md
├── src/
│   ├── ConnectionWatcher.Core/
│   └── ConnectionWatcher.App/
├── tests/
│   ├── ConnectionWatcher.Tests/
│   └── ConnectionWatcher.UiSmoke/
├── docs/
├── learning/
├── scripts/
│   └── build-release.ps1
├── packaging/
└── Final-Share/
    ├── TCP-Connection-Watcher-Setup-win-x64.exe
    ├── SHA256SUMS.txt
    └── Docs/
```

- `ConnectionWatcher.sln`: the solution file for the entire project.
- `src/ConnectionWatcher.Core`: core logic for settings, rules, Windows TCP connection reading, time-based connection tracking, process context, and backward-compatible CSV logs.
- `src/ConnectionWatcher.App`: the seven-language Windows interface, including the main window, rule editor, event details, built-in help center, update checker, tray notices, and alert window.
- `tests`: 20 functional and compatibility tests, plus multilingual interface and DPI-scaling tests.
- `docs`: project overviews and user guides in all seven supported languages.
- `learning`: the developer tutorial and architecture learning material.
- `scripts/build-release.ps1`: runs verification and automatically produces `artifacts`, `dist`, and `Final-Share` in order.
- `packaging`: installer definitions and portable-edition notes.
- `Final-Share`: the local, Git-ignored sharing folder, with one multilingual installer, all seven document sets, release notes, and SHA-256 checksums.

## Build and verification

Building the source on Windows requires the .NET 8 SDK.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Release packages include `SHA256SUMS.txt`, which recipients can verify with `Get-FileHash` in PowerShell.

Maintainers can run `scripts\build-release.ps1` to build, test, publish, package, copy the current documents, and generate the checksum in one workflow.
