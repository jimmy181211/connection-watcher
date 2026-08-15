# TCP-Verbindungsmonitor

## Hintergrund und Zweck

Bei der Untersuchung einer ungewöhnlichen Netzwerkverbindung muss häufig eine einfache Frage beantwortet werden, die sich nicht immer rechtzeitig prüfen lässt:

> Hat mein Computer eine Verbindung zu einer bestimmten IP-Adresse oder einem Port hergestellt? Wenn ja, wann, welchem Prozess ordnete Windows sie zu und welcher Anwendungskontext lässt sich ermitteln?

Der Windows-Ressourcenmonitor zeigt die aktuelle Netzwerkaktivität, muss aber geöffnet und ständig beobachtet werden. Eine kurze Verbindung kann schnell verschwinden, und eine dauerhafte Beobachtung ist unpraktisch. Außerdem warnt er nicht automatisch vor einem ausgewählten Ziel und führt kein fortlaufendes Protokoll darüber.

Der TCP-Verbindungsmonitor löst dieses Problem. Nachdem eine IP-Adresse oder ein Port ausgewählt wurde, sucht die App im Hintergrund nach passenden Verbindungen. Sie protokolliert Zeit, Adressen, Ports, den von Windows gemeldeten Verbindungsinhaber sowie verfügbare Datei-, übergeordnete Prozess- und Windows-Dienstinformationen und benachrichtigt den Benutzer entsprechend den Einstellungen.

Dieses Tool ersetzt weder den Ressourcenmonitor noch Antivirensoftware. Es hilft, ausgewählte Ziele zu überwachen, Informationen aufzubewahren und diese für eine spätere Sicherheitsuntersuchung bereitzustellen.

## Projektübersicht

Der TCP-Verbindungsmonitor ist ein kleines, regelbasiertes **Windows-Tool zur Beobachtung von Netzwerkverbindungen**. Benutzer können eine Remote-IP, einen Remote-Port oder einen lokalen Port auswählen. Meldet Windows eine TCP-Verbindung, die einer aktivierten Regel entspricht, wird sie protokolliert oder gemeldet.

Einfach gesagt überwacht das Tool eine bestimmte IP-Adresse oder einen Port. Es kann zum Beispiel `103.1.40.235:1433` beobachten. Sobald eine entsprechende Verbindung erscheint, zeichnet die App Zeit, aktiven oder beendeten Status, beobachtete Dauer, den von Windows gemeldeten Inhaber, PID und verfügbaren Anwendungskontext auf. Je nach Einstellung kann sie **im Hintergrund protokollieren, einen Taskleistenhinweis anzeigen oder ein Warnfenster öffnen.**

Das Standard-Prüfintervall beträgt eine Sekunde. Benutzer können in 0,5-Sekunden-Schritten einen Wert von 0,5 bis 10 Sekunden wählen. Ein kürzeres Intervall erkennt kurze Verbindungen eher; ein längeres benötigt weniger Ressourcen, kann sie aber übersehen.

Die App sagt nur: „Eine von Ihnen ausgewählte Verbindung ist erschienen.“ Sie kennzeichnet andere Verbindungen nicht als verdächtig, und eine einzelne Verbindung beweist keine Infektion. Die gespeicherten Daten können einem Cybersicherheitsteam zur weiteren Untersuchung übergeben werden.

## Projektstruktur

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

- `ConnectionWatcher.sln`: Projektmappendatei für das gesamte Projekt.
- `src/ConnectionWatcher.Core`: Kernlogik für Einstellungen, Regeln, Windows-TCP-Verbindungen, zeitbasierte Verbindungsverfolgung, Prozesskontext und rückwärtskompatible CSV-Protokolle.
- `src/ConnectionWatcher.App`: Windows-Oberfläche in sieben Sprachen mit Hauptfenster, Regeleditor, Ereignisdetails, integrierter Hilfe, Update-Prüfung, Taskleistenhinweisen und Warnfenster.
- `tests`: 20 Funktions- und Kompatibilitätstests sowie mehrsprachige Oberflächen- und DPI-Skalierungstests.
- `docs`: Projektübersichten und Benutzerhandbücher in sieben Sprachen.
- `learning`: Entwicklertutorial und Lernmaterial zur Architektur.
- `scripts/build-release.ps1`: führt Prüfungen aus und erzeugt automatisch nacheinander `artifacts`, `dist` und `Final-Share`.
- `packaging`: Installationsdefinition und Hinweise zur portablen Version.
- `Final-Share`: lokaler, von Git ignorierter Freigabeordner mit einem mehrsprachigen Installationsprogramm, allen sieben Dokumentsätzen, Versionshinweisen und SHA-256-Prüfsummen.

## Erstellen und Prüfen

Zum Erstellen unter Windows ist das .NET 8 SDK erforderlich.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Veröffentlichte Pakete enthalten `SHA256SUMS.txt`. Empfänger können die Prüfsummen mit `Get-FileHash` in PowerShell überprüfen.

Verantwortliche können `scripts\build-release.ps1` ausführen, um Build, Tests, Veröffentlichung, Paketierung, das Kopieren der aktuellen Dokumente und die Prüfsummenerzeugung in einem Ablauf zu erledigen.
