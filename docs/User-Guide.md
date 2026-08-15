# SocketSight User Guide

## Contents

- [What is this tool?](#what-is-this-tool)
- [Installation and quick start](#installation-and-quick-start)
- [Check interval](#check-interval)
- [What happens after a match?](#what-happens-after-a-match)
- [Viewing events](#viewing-events)
- [Understanding a record](#understanding-a-record)
- [Help center and updates](#help-center-and-updates)
- [Logs, sound, and other settings](#logs-sound-and-other-settings)
- [Privacy, permissions, and uninstall](#privacy-permissions-and-uninstall)

## What is this tool?

SocketSight helps you watch a specific IP address or port.

When a TCP connection matches a rule, the app records its time, IP, port, and process information available from Windows, then follows your alert setting.

It only observes, records, and alerts. It does not close programs, change the firewall, or block an IP address.

## Installation and quick start

The language selected during installation is also used by the app. When upgrading, choosing a different language changes the app language once; existing rules, settings, and logs remain.

If startup takes longer than about 0.5 seconds, SocketSight shows a short startup screen. It closes when the main window is ready.

1. Open **Monitoring Rules**.
2. Select **New rule**.
3. Enter the IP address or port to watch.
4. Save and enable the rule.
5. Return to **Home** and select **Start monitoring**.

For example, to watch `103.1.40.235:1433`:

- Remote IP: `103.1.40.235`
- Remote port: `1433`
- Local port: Any
- Action: Pop up alert and log
- Repeat alert interval: 5 minutes

## Check interval

The default interval is one second. On **Home**, you can choose 0.5–10 seconds in 0.5-second steps.

A shorter interval is more likely to catch a brief connection but uses more resources. Even at 0.5 seconds, a connection that appears and disappears between two checks can be missed.

Only enabled rules create records or alerts.

## What happens after a match?

- **Log silently:** writes to the log without an alert.
- **Tray notice and log:** changes the tray icon to a warning state; opening the Event Log clears the notice.
- **Pop-up alert and log:** shows a window for the first match; later matches update the same window.

The numbers and shapes on Home and in the event list help distinguish these three actions.

## Viewing events

The same connection appears as one record, not a new row every second.

- An existing connection shows **Active**.
- A finished connection shows **Ended**.
- **Observed duration** updates while active and stops changing after the connection ends.
- **Application** shows the file product name when available; otherwise it shows the process name.
- Double-click a record to see the process, PID, path, parent processes, Windows services, and other details. You can also copy the record.

A connection is marked ended after it has been absent from the Windows list for two seconds. If it returns within two seconds, it remains the same record; a later return creates a new record.

The duration starts when the app first sees the connection, so it may not equal the connection's actual lifetime. The app cannot observe while monitoring is stopped; starting again creates a new record.

## Understanding a record

A rule match only means that a connection you chose to watch appeared. It does not prove that the computer has malware.

Browsers, proxies, VPNs, or web components may already be running in the background. Process information can help identify a related application, but it cannot guarantee which application ultimately caused the connection.

The TCP connection list cannot reliably show which side initiated a connection. Windows permissions may also prevent the app from reading some paths, file details, parent processes, or services.

For a security decision, combine these records with antivirus scans or professional advice.

## Help center and updates

In **Settings**, select **Open** beside Help Center to read the project overview and user guide. The documents follow the current interface language.

Select **Check now** to ask GitHub for a newer public release. The app does not download, install, or run updates automatically.

In **Settings**, open **Feedback** to write a suggestion or problem report. The app opens a pre-filled GitHub Issue page in your browser. Review the text and submit it yourself. Logs and connection records are not attached by default.

## Logs, sound, and other settings

Logs are stored in:

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

CSV is written when a connection is found and when its observation ends, not every second. The Event Log combines the same connection into one row.

**Clear display** hides rows from the Event Log without deleting CSV files. The old rows stay hidden after a restart; new events appear normally.

The default log limit is 25 MB. You can change it to 5–500 MB in **Settings**. The app keeps up to five files and removes the oldest file when the limit is reached.

**Launch app at Windows sign-in** opens the app but does not start monitoring. **Start monitoring automatically when the app opens** starts monitoring with enabled rules.

The urgent alert sound is used for pop-up alerts. You can adjust its volume in **Settings**; **Test sound** uses the same volume, and Windows system volume still applies.

## Privacy, permissions, and uninstall

- Administrator rights, an account, and a password are not required.
- The app does not read packet contents.
- Rules and logs are not uploaded.
- GitHub is contacted only when you manually check for updates or open the feedback page.

When uninstalled, settings and logs are kept by default. If you no longer need them, you can manually delete:

```text
%LOCALAPPDATA%\ConnectionWatcher
```
