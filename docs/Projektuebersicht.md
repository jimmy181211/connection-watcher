# TCP-Verbindungsmonitor

## Hintergrund und Zweck

Bei der Untersuchung einer ungewöhnlichen Netzwerkverbindung muss häufig eine einfache Frage beantwortet werden, die sich nicht immer rechtzeitig prüfen lässt:

> Hat mein Computer eine Verbindung zu einer bestimmten IP-Adresse oder einem Port hergestellt? Wenn ja, wann und durch welches Programm?

Der Windows-Ressourcenmonitor zeigt die aktuelle Netzwerkaktivität, muss aber geöffnet und ständig beobachtet werden. Eine kurze Verbindung kann schnell verschwinden, und eine dauerhafte Beobachtung ist unpraktisch. Außerdem warnt er nicht automatisch vor einem ausgewählten Ziel und führt kein fortlaufendes Protokoll darüber.

Der TCP-Verbindungsmonitor löst dieses Problem. Nachdem eine IP-Adresse oder ein Port ausgewählt wurde, sucht die App im Hintergrund nach passenden Verbindungen. Sie protokolliert Zeit, Adressen, Ports sowie verfügbare Programm- und PID-Informationen und benachrichtigt den Benutzer entsprechend den Einstellungen.

Dieses Tool ersetzt weder den Ressourcenmonitor noch Antivirensoftware. Es hilft, ausgewählte Ziele zu überwachen, Informationen aufzubewahren und diese für eine spätere Sicherheitsuntersuchung bereitzustellen.

## Projektübersicht

Der TCP-Verbindungsmonitor ist ein kleines, regelbasiertes **Windows-Tool zur Beobachtung von Netzwerkverbindungen**. Benutzer können eine Remote-IP, einen Remote-Port oder einen lokalen Port auswählen. Meldet Windows eine TCP-Verbindung, die einer aktivierten Regel entspricht, wird sie protokolliert oder gemeldet.

Einfach gesagt überwacht das Tool eine bestimmte IP-Adresse oder einen Port. Es kann zum Beispiel `103.1.40.235:1433` beobachten. Sobald eine entsprechende Verbindung erscheint, zeichnet die App Zeit, aktiven oder beendeten Status, beobachtete Dauer, Programm und PID auf. Je nach Einstellung kann sie **im Hintergrund protokollieren, einen Taskleistenhinweis anzeigen oder ein Warnfenster öffnen.**

Die App sagt nur: „Eine von Ihnen ausgewählte Verbindung ist erschienen.“ Sie kennzeichnet andere Verbindungen nicht als verdächtig, und eine einzelne Verbindung beweist keine Infektion. Die gespeicherten Daten können einem Cybersicherheitsteam zur weiteren Untersuchung übergeben werden.

## Projektstruktur

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
    ├── TCP-Connection-Watcher-Setup-win-x64.exe
    ├── SHA256SUMS.txt
    └── Docs/
```

- `ConnectionWatcher.sln`: Projektmappendatei für das gesamte Projekt.
- `src/ConnectionWatcher.Core`: Kernlogik für Einstellungen, Regeln, Windows-TCP-Verbindungen, Deduplizierung und CSV-Protokolle.
- `src/ConnectionWatcher.App`: Windows-Oberfläche in sieben Sprachen mit Hauptfenster, Regeleditor, integrierter Hilfe, Taskleistenhinweisen und Warnfenster.
- `tests`: Funktions- und Oberflächentests; die Funktionssuite umfasst derzeit 16 Tests.
- `docs`: Projektübersichten und Benutzerhandbücher in sieben Sprachen.
- `packaging`: Installationsdefinition und Hinweise zur portablen Version.
- `Final-Share`: Freigabeordner mit einem mehrsprachigen Installationsprogramm, Dokumenten und SHA-256-Prüfsummen.

## Erstellen und Prüfen

Zum Erstellen unter Windows ist das .NET 8 SDK erforderlich.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Veröffentlichte Pakete enthalten `SHA256SUMS.txt`. Empfänger können die Prüfsummen mit `Get-FileHash` in PowerShell überprüfen.
