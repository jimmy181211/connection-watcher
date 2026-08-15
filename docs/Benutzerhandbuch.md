# Benutzerhandbuch für den TCP-Verbindungsmonitor

## Hauptzweck

Dieses Tool hilft Ihnen, **eine selbst gewählte IP-Adresse oder einen Port zu überwachen**. Es kann:

- Das Erscheinen einer Verbindung automatisch protokollieren
- Lokale und entfernte IP-Adressen und Ports aufzeichnen
- Wenn verfügbar, den von Windows gemeldeten Verbindungsinhaber, PID, Programmpfad, Dateiinformationen, übergeordnete oder Hostprozesse und zugehörige Windows-Dienste erfassen
- Je nach Einstellung im Hintergrund protokollieren, einen Taskleistenhinweis oder ein Warnfenster anzeigen
- Informationen zur späteren Prüfung oder Weitergabe an Sicherheitspersonal speichern
- Bestätigen, ob später eine neue Verbindung zum gleichen Ziel erscheint

## Funktionsweise

Erstellen Sie zuerst eine Regel für die gewünschte IP-Adresse oder den Port. Aktivieren Sie die Regel und starten Sie die Überwachung. Standardmäßig prüft die App die Windows-Liste der TCP-Verbindungen einmal pro Sekunde. Auf der **Startseite** können Sie das Intervall in 0,5-Sekunden-Schritten von 0,5 bis 10 Sekunden einstellen. Kürzere Intervalle erkennen kurze Verbindungen eher; längere benötigen weniger Ressourcen, können sie aber übersehen. Nur Verbindungen, die einer aktivierten Regel entsprechen, werden verarbeitet; andere Verbindungen erzeugen keine Einträge oder Warnungen.

Bei einem Treffer führt die App die ausgewählte Aktion aus:

- **Im Hintergrund protokollieren:** Schreibt das Ereignis in das CSV-Protokoll, ohne das Taskleistensymbol zu ändern oder eine Anzahl anzuzeigen.
- **Taskleistenhinweis und Protokoll:** Öffnet kein Fenster. Das Symbol wechselt in den Warnzustand; beim Öffnen des Ereignisprotokolls wird der Hinweis gelöscht.
- **Warnfenster und Protokoll:** Zeigt beim ersten Treffer sofort ein Fenster. Solange es geöffnet ist, werden weitere Treffer im selben Fenster ergänzt. Nach dem Schließen bestimmt das Regelintervall, wann erneut gewarnt wird.

Die Startseite zeigt für jede Aktion ein kompaktes Symbol. **Überwachungsregeln** kombiniert Symbol und Kurzname; die Spalte **Aktion** im Ereignisprotokoll zeigt nur das Symbol:

- `1 ●` grauer Kreis: Im Hintergrund protokollieren
- `2 ▲` orangefarbenes Dreieck: Taskleistenhinweis und Protokoll
- `3 ◆` rote Raute: Warnfenster und Protokoll

Zahl und Form unterscheiden die Aktionen auch ohne Farbe. Zeigen Sie mit der Maus auf ein Symbol, um den vollständigen Namen anzuzeigen.

#### *Wichtiger Hinweis:*

1. Ein Treffer bedeutet nur, dass eine ausgewählte Verbindung erschienen ist. Er beweist keine Infektion des Computers.
2. Dieses Tool **protokolliert Verbindungen und zeigt Warnungen an**. Weitere Sicherheitsmaßnahmen sollten auch Virenscans und den Rat qualifizierter Fachleute berücksichtigen.

## Erste Verwendung

1. Wählen Sie bei der Installation eine der sieben unterstützten Sprachen; die portable Version fragt beim ersten Start ebenfalls danach.
2. Öffnen Sie **Überwachungsregeln**.
3. Wählen Sie **Neue Regel**.
4. Geben Sie die Bedingungen in die Formularfelder ein.
5. Prüfen Sie die Vorschau am unteren Rand.
6. Speichern und aktivieren Sie die Regel.
7. Gehen Sie zur **Startseite** und wählen Sie **Überwachung starten**.

### Beispiel

Um zu überwachen, ob sich ein beliebiger lokaler Port erneut mit `103.1.40.235:1433` verbindet, erstellen Sie folgende Regel:

- Regeltyp: TCP-Verbindung
- Remote-IP: `103.1.40.235`
- Remote-Port: `1433`
- Lokaler Port: Beliebig
- Aktion: Warnfenster und Protokoll
- Wiederholungsintervall: 5 Minuten

## Protokolle

Protokolle werden gespeichert unter:

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

Jede neue passende Verbindung erscheint als eine Zeile im **Ereignisprotokoll**. Bleibt sie mehrere Stunden geöffnet, wird sie nicht jede Sekunde erneut protokolliert. **Status** zeigt an, ob sie aktiv oder beendet ist. **Beobachtete Dauer** wird während der Aktivität aktualisiert und nach dem Ende festgehalten.

Zur besseren Lesbarkeit zeigt die Tabelle nur die wichtigsten Felder. Die Spalte **Anwendung** nutzt verfügbare Produktinformationen der Datei und zeigt andernfalls den Prozessnamen. Doppelklicken Sie auf eine Zeile, um die **Ereignisdetails** mit dem von Windows gemeldeten Verbindungsinhaber, PID, Pfad, Produktinformationen, bis zu drei übergeordneten oder Hostprozessen, zugehörigen Windows-Diensten und den übrigen Verbindungsfeldern zu öffnen. Aktiver Status und Dauer werden dort weiter aktualisiert; **Details kopieren** kopiert den vollständigen Datensatz.

Dieser Kontext kann helfen, die mit einer Verbindung zusammenhängende Anwendung zu erkennen, beweist aber nicht immer, welche Anwendung sie letztlich ausgelöst hat. Beispielsweise kann ein Browser, Proxy, VPN oder eingebettete Webkomponente bereits im Hintergrund laufen.

Die beobachtete Dauer beginnt mit der ersten Erkennung und entspricht daher möglicherweise nicht der vollständigen tatsächlichen Dauer. Während einer gestoppten Überwachung kann die App keine Unterbrechung erkennen; ein erneuter Start erzeugt deshalb eine neue Beobachtung. Das interne CSV schreibt Lebenszyklusdaten nur beim Erkennen und beim Ende. Die App fasst diese Daten zu einer Zeile zusammen.

Eine Verbindung wird erst als beendet markiert, nachdem sie zwei Sekunden lang nicht in der Windows-Verbindungstabelle vorkam. Erscheint sie innerhalb dieser Wartezeit wieder, bleibt sie dieselbe Beobachtung. Als Endzeit gilt der letzte Zeitpunkt, zu dem die App die Verbindung tatsächlich gesehen hat. Ein späteres Erscheinen nach der Wartezeit erzeugt einen neuen Eintrag.

Wählen Sie **Anzeige leeren**, wenn das Ereignisprotokoll übersichtlich bleiben soll. Dadurch werden vorhandene Zeilen in der Oberfläche ausgeblendet, ohne die CSV-Protokolle zu löschen. Frühere Ereignisse bleiben auch nach einem Neustart der App verborgen; neu erkannte Ereignisse werden normal angezeigt.

Das Gesamtlimit beträgt standardmäßig 25 MB und kann unter **Einstellungen** auf 5–500 MB geändert werden. Die App verwendet bis zu fünf Dateien und entfernt beim Erreichen des Limits die ältesten Einträge.

## Hilfe

Wählen Sie unter **Einstellungen** die Option **Hilfe öffnen**, um Projektübersicht und Benutzerhandbuch in der aktuellen Oberflächensprache zu lesen.

## Softwareupdates

Wählen Sie unter **Einstellungen** die Option **Jetzt prüfen**, um GitHub nach der neuesten öffentlichen Version zu fragen. Die App tut dies nur auf Ihre Anforderung. Ist eine neuere Version verfügbar, können Sie deren GitHub-Release-Seite öffnen, die Versionshinweise lesen und sie selbst herunterladen. Die App lädt, installiert oder startet Updates nicht automatisch und überträgt weder Regeln noch Protokolle.

## Start- und Toneinstellungen

- **App nach der Windows-Anmeldung starten:** Öffnet die App nach der Anmeldung, startet aber nicht automatisch die Überwachung.
- **Überwachung beim Öffnen automatisch starten:** Startet mit den aktivierten Regeln.
- **Ton bei dringender Warnung:** Verwendet einen integrierten kurzen Ton, unabhängig vom Windows-Ereignisschema. Die Lautstärke ist von 10 % bis 100 % einstellbar (Standard 40 %). **Ton testen** steht neben der Lautstärkeregelung; Test und echte dringende Warnungen verwenden denselben Wert, und die Windows-Lautstärke gilt weiterhin.

## Wichtige Einschränkungen

1. Die App prüft standardmäßig einmal pro Sekunde. Selbst bei der Einstellung 0,5 Sekunden kann eine Verbindung übersehen werden, die zwischen zwei Prüfungen erscheint und wieder verschwindet.
2. Version 1 **überwacht nur TCP**, nicht UDP.
3. Die Windows-TCP-Tabelle liefert keine vollständig zuverlässige Angabe darüber, welche Seite die Verbindung gestartet hat.
4. Windows-Berechtigungen oder ein schnell beendeter Prozess können verhindern, dass Pfad, Dateiinformationen, übergeordneter Prozess oder zugehöriger Dienst gelesen werden. PID und verfügbare Prozessnamen werden weiterhin erfasst. Prozess- und Dienstkontext sind Hinweise für die Untersuchung, keine garantierte Aussage zur eigentlichen Ursache.
5. Bei geschlossener App, gestoppter Überwachung oder im Energiesparmodus findet keine Überwachung statt.
6. Die beobachtete Dauer beginnt mit der ersten Erkennung. Ihre Genauigkeit hängt vom gewählten Prüfintervall ab; sie ist keine von Windows gelieferte genaue Startzeit.
7. Die App beendet keine Programme, ändert keine Firewallregeln und blockiert keine IP-Adressen.

## Datenschutz und Berechtigungen

1. Administratorrechte sind nicht erforderlich.
2. Anmeldung, Benutzername, Kennwort oder E-Mail-Adresse sind nicht erforderlich.
3. Die App verbindet sich nur dann mit GitHub, wenn Sie manuell **Jetzt prüfen** wählen. Sie verbindet sich nicht mit einem Entwicklerserver und lädt weder Regeln noch Protokolle hoch.
4. Sie liest keine Paketinhalte.
5. Einstellungen werden in `%LOCALAPPDATA%\ConnectionWatcher\config.json` gespeichert.

## Deinstallation

Sie können die installierte Version unter **Installierte Apps** in Windows entfernen. Die Deinstallation entfernt das Programm, behält aber standardmäßig Einstellungen und Protokolle in `%LOCALAPPDATA%\ConnectionWatcher`, damit Untersuchungsdaten nicht versehentlich verloren gehen. Löschen Sie diesen Ordner manuell, wenn Sie die Daten nicht mehr benötigen.
