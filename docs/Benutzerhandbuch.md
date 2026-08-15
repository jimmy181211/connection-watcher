# SocketSight – Benutzerhandbuch

## Inhalt

- [Was ist dieses Programm?](#was-ist-dieses-programm)
- [Installation und Schnellstart](#installation-und-schnellstart)
- [Prüfintervall](#prüfintervall)
- [Was passiert bei einem Treffer?](#was-passiert-bei-einem-treffer)
- [Ereignisse anzeigen](#ereignisse-anzeigen)
- [Ein Ereignis verstehen](#ein-ereignis-verstehen)
- [Hilfe und Updates](#hilfe-und-updates)
- [Protokolle, Ton und weitere Einstellungen](#protokolle-ton-und-weitere-einstellungen)
- [Datenschutz, Rechte und Deinstallation](#datenschutz-rechte-und-deinstallation)

## Was ist dieses Programm?

SocketSight hilft Ihnen, eine bestimmte IP-Adresse oder einen Port zu beobachten.

Wenn eine TCP-Verbindung eine Regel erfüllt, speichert das Programm Zeit, IP, Port und verfügbare Windows-Prozessinformationen und verwendet die gewählte Benachrichtigung.

Es beobachtet, protokolliert und benachrichtigt nur. Es schließt keine Programme, ändert keine Firewall und blockiert keine IP-Adresse.

## Installation und Schnellstart

Die bei der Installation gewählte Sprache wird auch in der Anwendung verwendet. Bei einer Aktualisierung ändert eine neue Auswahl die Sprache einmal; Regeln, Einstellungen und Protokolle bleiben erhalten.

Wenn der Start länger als etwa 0,5 Sekunden dauert, zeigt SocketSight einen kurzen Startbildschirm, der beim Bereitstehen des Hauptfensters verschwindet.

1. Öffnen Sie **Überwachungsregeln**.
2. Wählen Sie **Neue Regel**.
3. Geben Sie die zu beobachtende IP oder den Port ein.
4. Speichern und aktivieren Sie die Regel.
5. Gehen Sie zu **Startseite** und wählen Sie **Überwachung starten**.

Beispiel für `103.1.40.235:1433`:

- Remote-IP: `103.1.40.235`
- Remote-Port: `1433`
- Lokaler Port: Beliebig
- Aktion: Warnfenster und Protokoll
- Wiederholungsintervall: 5 Minuten

## Prüfintervall

Standardmäßig wird jede Sekunde geprüft. Auf der **Startseite** können Sie 0,5–10 Sekunden in 0,5-Sekunden-Schritten wählen.

Ein kurzes Intervall erkennt kurze Verbindungen eher, verbraucht aber mehr Ressourcen. Auch bei 0,5 Sekunden kann eine Verbindung verpasst werden, die zwischen zwei Prüfungen erscheint und verschwindet.

Nur aktivierte Regeln erzeugen Einträge oder Hinweise.

## Was passiert bei einem Treffer?

- **Im Hintergrund protokollieren:** schreibt in das Protokoll, ohne zu benachrichtigen.
- **Taskleistenhinweis und Protokoll:** das Tray-Symbol wechselt in den Warnzustand; das Öffnen des Ereignisprotokolls entfernt den Hinweis.
- **Warnfenster und Protokoll:** zeigt beim ersten Treffer ein Fenster; weitere Treffer aktualisieren dieses Fenster.

Zahlen und Formen auf der Startseite und in der Ereignisliste helfen, die drei Aktionen zu unterscheiden.

## Ereignisse anzeigen

Dieselbe Verbindung wird nur als ein Eintrag angezeigt, nicht jede Sekunde als neue Zeile.

- Eine vorhandene Verbindung ist **Aktiv**.
- Eine beendete Verbindung ist **Beendet**.
- **Beobachtete Dauer** wird während der Aktivität aktualisiert und bleibt danach fest.
- **Anwendung** zeigt, wenn möglich, den Produktnamen der Datei, sonst den Prozessnamen.
- Doppelklicken Sie auf einen Eintrag, um Prozess, PID, Pfad, Elternprozesse, Windows-Dienste und weitere Details zu sehen. Der Eintrag kann kopiert werden.

Nach zwei Sekunden Abwesenheit aus der Windows-Liste wird eine Verbindung beendet. Wenn sie innerhalb von zwei Sekunden zurückkommt, bleibt es derselbe Eintrag; später wird ein neuer Eintrag angelegt.

Die Dauer beginnt, wenn die Anwendung die Verbindung erstmals sieht, und entspricht daher nicht unbedingt ihrer tatsächlichen Lebensdauer. Während die Überwachung gestoppt ist, wird nicht beobachtet; beim Neustart entsteht ein neuer Eintrag.

## Ein Ereignis verstehen

Ein Regel-Treffer bedeutet nur, dass eine von Ihnen ausgewählte Verbindung aufgetreten ist. Er beweist keine Malware.

Browser, Proxy, VPN oder Webkomponenten können bereits im Hintergrund laufen. Prozessinformationen helfen bei der Suche nach einer verbundenen Anwendung, beweisen aber nicht, welche Anwendung die Verbindung letztlich ausgelöst hat.

Die TCP-Liste kann nicht zuverlässig zeigen, welche Seite die Verbindung gestartet hat. Windows-Berechtigungen können außerdem den Zugriff auf Pfade, Dateidaten, Elternprozesse oder Dienste verhindern.

Für eine Sicherheitsentscheidung sollten Sie die Daten mit einem Virenscan oder professioneller Beratung kombinieren.

## Hilfe und Updates

Wählen Sie in **Einstellungen** neben dem Hilfezentrum **Öffnen**, um Projektübersicht und Benutzerhandbuch zu lesen. Die Dokumente folgen der aktuellen Sprache.

Wählen Sie **Jetzt prüfen**, um GitHub nach einer neueren öffentlichen Version zu fragen. Die Anwendung lädt, installiert oder startet Updates nicht automatisch.

Öffnen Sie in **Einstellungen** **Feedback**, um einen Vorschlag oder ein Problem zu schreiben. Der Browser öffnet eine vorausgefüllte GitHub-Issue; prüfen Sie den Text und senden Sie sie selbst. Protokolle und Verbindungen werden standardmäßig nicht angehängt.

## Protokolle, Ton und weitere Einstellungen

Protokolle werden gespeichert unter:

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

Die CSV-Datei wird beim Erkennen einer Verbindung und beim Ende ihrer Beobachtung geschrieben, nicht jede Sekunde. Das Ereignisprotokoll fasst dieselbe Verbindung in einer Zeile zusammen.

**Anzeige leeren** blendet Zeilen aus, ohne CSV-Dateien zu löschen. Alte Zeilen bleiben nach einem Neustart verborgen; neue Ereignisse erscheinen normal.

Das Standardlimit beträgt 25 MB und kann in **Einstellungen** auf 5–500 MB geändert werden. Bis zu fünf Dateien bleiben erhalten; bei Erreichen des Limits wird die älteste gelöscht.

**App nach der Windows-Anmeldung starten** öffnet nur die App. **Überwachung beim Öffnen automatisch starten** beginnt mit den aktivierten Regeln.

Der dringende Warnton wird für Warnfenster verwendet. Die Lautstärke kann in **Einstellungen** geändert werden; **Ton testen** verwendet dieselbe Lautstärke, und die Windows-Lautstärke gilt zusätzlich.

## Datenschutz, Rechte und Deinstallation

- Administratorrechte, Konto und Passwort sind nicht erforderlich.
- Die Anwendung liest keine Paketdaten.
- Regeln und Protokolle werden nicht hochgeladen.
- GitHub wird nur bei manueller Updateprüfung oder beim Öffnen der Feedbackseite kontaktiert.

Bei der Deinstallation bleiben Einstellungen und Protokolle standardmäßig erhalten. Wenn Sie sie nicht mehr benötigen, löschen Sie manuell:

```text
%LOCALAPPDATA%\ConnectionWatcher
```
