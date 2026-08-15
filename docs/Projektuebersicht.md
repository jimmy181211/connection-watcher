# SocketSight – Projektübersicht

## Inhalt

- [Hintergrund und Zweck](#hintergrund-und-zweck)
- [Projektübersicht](#projektübersicht)
- [Wichtige Entwurfsentscheidungen](#wichtige-entwurfsentscheidungen)
- [Projektstruktur](#projektstruktur)
- [Start, Sprache und Hilfe](#start-sprache-und-hilfe)
- [Erstellen und Prüfen](#erstellen-und-prüfen)

## Hintergrund und Zweck

Der Windows-Ressourcenmonitor zeigt die aktuelle Netzwerkaktivität, muss dafür aber geöffnet und ständig beobachtet werden. Eine kurze Verbindung kann verschwinden, bevor sie bemerkt wird, und eine gezielte Langzeitaufzeichnung ist umständlich.

SocketSight ermöglicht Regeln für eine entfernte IP-Adresse, einen entfernten Port oder einen lokalen Port. Es verarbeitet nur passende TCP-Verbindungen und speichert Zeitpunkt, Status, beobachtete Dauer, den von Windows gemeldeten Prozess und verfügbare Anwendungsinformationen.

Das Programm ersetzt weder den Ressourcenmonitor noch ein Antivirenprogramm. Es erleichtert die wiederholte Beobachtung einer ausgewählten Verbindung und die spätere Untersuchung durch den Benutzer oder Fachpersonal.

## Projektübersicht

SocketSight ist ein lokal unter Windows laufendes TCP-Beobachtungsprogramm mit regelbasierter Auswahl. Nach dem Start der Überwachung liest es die Windows-TCP-Tabelle im eingestellten Intervall und verarbeitet Verbindungen, die aktivierte Regeln erfüllen.

Das Standardintervall beträgt eine Sekunde. Es kann in Schritten von 0,5 Sekunden auf 0,5–10 Sekunden eingestellt werden. Ein kürzeres Intervall erkennt kurze Verbindungen eher, benötigt aber mehr Prüfungen; ein längeres spart Ressourcen, kann kurze Verbindungen jedoch verpassen.

Die Anwendung verarbeitet nur Verbindungen, die durch Regeln ausgewählt wurden. Andere Netzwerkaktivität wird nicht automatisch als verdächtig bezeichnet. Diese Version konzentriert sich auf TCP; UDP würde ein anderes, tieferes Tracing und eine deutlich komplexere Zuordnung zu Anwendungen benötigen.

## Wichtige Entwurfsentscheidungen

- **Regeln zuerst:** Nur Verbindungen mit aktivierten passenden Regeln werden verarbeitet.
- **Eine Beobachtung pro Verbindung:** Eine dauerhafte Verbindung wird nicht jede Sekunde erneut geschrieben.
- **Ende nach Echtzeit:** Nach zwei Sekunden Abwesenheit gilt sie als beendet; bei Rückkehr innerhalb dieser Zeit bleibt es dieselbe Beobachtung.
- **Anwendungskontext ist ein Hinweis:** Prozess-, PID-, Datei-, Elternprozess- und Dienstinformationen helfen bei der Untersuchung, beweisen aber nicht die endgültige Ursache.
- **Ansicht und Daten getrennt:** **Anzeige leeren** blendet alte Zeilen aus, ohne CSV-Dateien zu löschen.
- **Lokal:** Das Programm liest keine Paketdaten und lädt Regeln oder Protokolle nicht hoch. GitHub wird nur bei manueller Updateprüfung oder beim Öffnen der Feedbackseite kontaktiert.

## Projektstruktur

```text
connection-watcher/
├── ConnectionWatcher.sln
├── RELEASE_NOTES.md
├── src/
│   ├── ConnectionWatcher.Core/       # Regeln, Überwachung, Status, Protokolle, Einstellungen
│   └── ConnectionWatcher.App/        # WinForms-Oberfläche, Sprachen, Tray, Start
├── tests/
│   ├── ConnectionWatcher.Tests/      # Kern- und Kompatibilitätstests
│   └── ConnectionWatcher.UiSmoke/    # Sprach-, DPI- und Layouttests
├── docs/                             # Projektübersichten und Benutzerhandbücher
├── learning/                         # Entwicklertutorial und Lernmaterial
├── scripts/build-release.ps1         # Erstellen, Testen, Paketieren und Veröffentlichung
├── packaging/                        # Inno-Setup-Installationsdefinition
└── Final-Share/                      # fertige Dateien für Benutzer
```

- `ConnectionWatcher.Core` enthält Regeln, Windows-TCP-Lesen, Verbindungsverfolgung, Prozesskontext, CSV-Protokolle und Einstellungen.
- `ConnectionWatcher.App` enthält Oberfläche, Regelbearbeitung, Ereignisdetails, Hilfe, Hinweise, Warnungen, Sprachen und Startbildschirm.
- `tests` schützt das Kernverhalten und prüft verschiedene Sprachen und Anzeigeskalierungen.
- `scripts` erstellt, testet und veröffentlicht die eigenständige Anwendung, erzeugt den Installer, kopiert aktuelle Dokumente und erstellt SHA-256-Prüfsummen.
- `artifacts` ist die rohe Veröffentlichungs-Ausgabe, `dist` die Installer-Ausgabe und `Final-Share` das fertige Benutzerpaket. Alle drei können neu erstellt werden.

Benutzer laden einen Installer herunter: `SocketSight-Setup-win-x64.exe`. Die installierte Anwendung ist eigenständig und mehrteilig; eine separate .NET-Laufzeit ist nicht erforderlich.

## Start, Sprache und Hilfe

Der Installer unterstützt sieben Sprachen. Die bei der Installation gewählte Sprache wird auch zur Sprache der SocketSight-Oberfläche. Bei einer Aktualisierung ersetzt eine neue Auswahl die bisherige Sprache einmal; Regeln, Einstellungen und Protokolle bleiben erhalten.

Wenn der Start länger als etwa 0,5 Sekunden dauert, zeigt SocketSight einen kurzen lokalen Startbildschirm. Die wechselnden Texte sind nur Statusmeldungen und bedeuten weder eine Internetverbindung noch einen zusätzlichen Scan. Der Bildschirm schließt sich, sobald das Hauptfenster bereit ist.

Das Hilfezentrum in den Einstellungen zeigt Projektübersicht und Benutzerhandbuch in der aktuellen Sprache. Updates werden nur manuell geprüft und nicht automatisch heruntergeladen, installiert oder ausgeführt.

## Erstellen und Prüfen

Für den Windows-Build werden das .NET 8 SDK und Inno Setup benötigt.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Maintainer können ausführen:

```powershell
scripts\build-release.ps1
```

Das Skript erstellt, testet und veröffentlicht die Anwendung, erzeugt den Installer, sammelt aktuelle Dokumente und erstellt SHA-256-Prüfsummen. Empfänger können den Installer mit `Get-FileHash` in PowerShell prüfen.
