# TCP Connection Watcher

## Background and purpose

When investigating an unusual network connection, we often need to answer a simple question that can be difficult to confirm in time:

> Has my computer connected to a specific IP address or port? If it has, when did it happen, and which program created the connection?

Windows Resource Monitor can show current network activity, but the user must open it and keep watching it. A short connection may disappear quickly, and watching the window for a long time is not practical. It also does not automatically alert the user about a selected target or keep a continuous record of it.

TCP Connection Watcher helps solve this problem. After the user selects an IP address or port to watch, the app checks for matching connections in the background. When it finds one, it records the time, addresses, ports, and any available program and PID information, then alerts the user according to their settings.

This tool is not intended to replace Resource Monitor or antivirus software. It helps users monitor selected targets, keep records, and provide information for a later security investigation.

## Project overview

TCP Connection Watcher is a small, rule-based **Windows network connection monitoring tool**. Users can choose the remote IP address, remote port, or local port they care about. When Windows reports a TCP connection that matches an enabled rule, the app records it or alerts the user according to that rule.

Simply put, it helps you watch a specified IP address or port. For example, you can tell it to watch `103.1.40.235:1433`. After monitoring starts, if the computer connects to that target, the app records the connection time, the program using it, and its PID. Depending on the settings, it can **log silently, show a tray notice, or display a pop-up alert.**

The app only tells you, “A connection you asked me to watch has appeared.” It does not label other network connections as suspicious, and one connection alone cannot prove that the computer has a virus. The saved information can be shared with a cybersecurity team for further investigation.

## Project structure

```text
connection-watcher/
├── ConnectionWatcher.sln
├── src/
│   ├── ConnectionWatcher.Core/
│   └── ConnectionWatcher.App/
├── tests/
│   ├── ConnectionWatcher.Tests/
│   └── ConnectionWatcher.UiSmoke/
├── docs/
├── packaging/
└── Final-Share/
    ├── English/
    └── 中文版/
```

- `ConnectionWatcher.sln`: the solution file for the entire project.
- `src/ConnectionWatcher.Core`: core logic for settings, rules, Windows TCP connection reading, connection deduplication, and CSV logs.
- `src/ConnectionWatcher.App`: the bilingual Windows interface, including the main window, rule editor, built-in help center, tray notices, and alert window.
- `tests`: functional and interface tests; the functional test suite currently contains 14 tests.
- `docs`: Chinese and English project overviews and user guides.
- `packaging`: installer definitions and portable-edition notes.
- `Final-Share`: the final folders for sharing, with separate Chinese and English installers, documents, and SHA-256 checksums.

## Build and verification

Building the source on Windows requires the .NET 8 SDK.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Release packages include `SHA256SUMS.txt`, which recipients can verify with `Get-FileHash` in PowerShell.
