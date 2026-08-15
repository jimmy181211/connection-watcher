# TCP Connection Watcher User Guide

## Main purpose

Simply put, this tool helps you **watch an IP address or port that you choose**. It can:

- Automatically record when a connection appears
- Record the local and remote IP addresses and ports
- Record the connection owner reported by Windows, PID, executable path, file information, parent or host processes, and related Windows services whenever available
- Log silently, show a tray notice, or display a pop-up alert according to your settings
- Save records for later review or for sharing with cybersecurity staff
- Help you confirm whether a new connection to the same target appears later

## How the tool works

First, create a rule to tell the app which IP address or port to watch. Then enable the rule and start monitoring. The app checks the Windows TCP connection list once per second by default. You can change the interval on **Home** from 0.5 to 10 seconds in 0.5-second steps. Shorter intervals are more likely to catch brief connections; longer intervals use fewer resources but may miss them. The app processes only connections that match an enabled rule. Other normal connections do not create records or alerts.

When a connection matches a rule, the app follows your selected action:

- **Log silently:** Writes the event to the CSV log without changing the tray icon or showing an unread count.
- **Tray notice and log:** Does not show a pop-up. The tray icon changes to a warning state, and opening the Event log page clears the notice.
- **Pop up alert and log:** Shows a window as soon as the first match appears. While the window is open, later matches update the same window. After it is closed, the rule's repeat interval controls when another alert can appear.

The Home page shows a compact symbol for each action. **Monitoring Rules** combines the symbol with a short name, while the **Action** column in the Event Log shows the symbol alone so it remains clear in a narrow column:

- `1 ●` gray circle: Log silently
- `2 ▲` orange triangle: Tray notice and log
- `3 ◆` red diamond: Pop up alert and log

The number and shape also distinguish the actions when color is difficult to see. Point to a rule or Event Log symbol to view its full action name.

#### *Important note:*

1. A rule match means only that a connection you chose to watch has appeared. It does not prove that the computer has a virus.
2. This tool **only records connections and shows alerts**. Decisions about further security action should also consider antivirus scan results and advice from qualified professionals.

## First run

1. Choose one of the seven supported languages during installation. A portable edition asks for the language when it first opens.
2. Open **Monitoring Rules**.
3. Select **New rule**.
4. Enter the monitoring conditions in the form fields.
5. Check the rule preview at the bottom of the form.
6. Save and enable the rule.
7. Return to **Home** and select **Start monitoring**.

### Example

To monitor whether any local port on your computer connects again to `103.1.40.235:1433` (the remote server's IP address and port), create this rule:

- Rule type: TCP connection
- Remote IP: `103.1.40.235`
- Remote port: `1433`
- Local port: Any
- Action on match: Pop up alert and log
- Repeat alert interval: 5 minutes

## Log records

Logs are stored in:

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

Each new matching connection appears as one row in the **Event Log**. If it stays open for several hours, it is not recorded again every second. **Status** shows whether it is active or ended, while **Observed duration** updates while it is active and becomes fixed after it ends.

For easier reading, the table shows only the main fields. Its **Application** column uses available file product information and falls back to the process name. Double-click a row to open **Event details**, where you can see the connection owner reported by Windows, PID, path, file product information, up to three parent or host processes, related Windows services, and the remaining connection fields. Active status and duration continue to update there, and **Copy details** copies the complete record.

This context can help identify which application is related to a connection, but it may not prove which app ultimately triggered it. For example, a browser, proxy, VPN, or embedded web component may already be running in the background.

Observed duration starts when the app first sees the connection, so it may not equal the connection's full lifetime. After monitoring stops, the app cannot tell whether the connection ended during that gap. Starting monitoring again therefore creates a new observation. The background CSV writes lifecycle information only when the connection is detected and when the observation ends; the app combines those records into one Event Log row.

A connection is marked ended only after it has been absent from the Windows connection table for two seconds. If it reappears during that grace period, it remains the same observation. The end time uses the last moment when the app actually saw the connection. A later appearance after the grace period creates a new record.

Select **Clear display** when you want an uncluttered Event Log. This hides existing rows from the interface without deleting the CSV logs. Earlier events stay hidden after the app restarts, while newly detected events appear normally.

The total log limit is 25 MB by default and can be changed to 5–500 MB in **Settings**. The app uses up to five log files and automatically removes the oldest records when the selected limit is reached.

## Help center

In **Settings**, select **Open help center** to read the project overview and user guide inside the app. The documents follow the current interface language.

## Software updates

In **Settings**, select **Check now** to ask GitHub for the latest public release. The app does this only when you request it. If a newer version exists, you can open its GitHub Release page, read the release notes, and download it yourself. The app does not download, install, or run updates automatically, and it does not upload rules or logs.

## Startup and alert-sound settings

- **Launch app at Windows sign-in:** Opens the app after you sign in, helping prevent forgotten monitoring sessions. It does not start monitoring by itself.
- **Start monitoring automatically when the app opens:** Starts monitoring with enabled rules whenever the app opens.
- **Urgent alert sound:** Uses a short sound built into the app, so it does not depend on the Windows event-sound scheme. Set its volume from 10% to 100% (40% by default). **Test sound** appears beside the volume control; it and real urgent alerts use the same setting, and the Windows volume still applies.

## Important limitations

1. The app checks every second by default. Even at the 0.5-second setting, a connection that appears and disappears between two checks may be missed.
2. Version 1 **monitors TCP only**. It does not monitor UDP.
3. The Windows TCP connection table does not provide a completely reliable connection-initiator field, so the app cannot determine which side started a connection.
4. Windows permissions or a process ending quickly may prevent the app from reading a path, file information, parent process, or related service. The PID and any available process name are still recorded. Parent and service context is investigative evidence, not a guaranteed root-cause verdict.
5. The app does not monitor while it is closed, monitoring is stopped, or the computer is asleep.
6. Observed duration begins when the app first detects a connection. Its precision depends on the selected check interval, and it is not an exact connection-start time supplied by Windows.
7. The app only records connections and shows alerts. It does not close programs, change firewall settings, or block IP addresses.

## Privacy and permissions

1. Administrator rights are not required.
2. No login, username, password, or email address is required.
3. The app connects to GitHub only after you manually select **Check now**. It does not connect to a developer server or upload rules or logs.
4. It does not read packet contents.
5. Settings are stored in `%LOCALAPPDATA%\ConnectionWatcher\config.json`.

## Uninstall

You can remove the installed version through **Installed apps** in Windows. Uninstalling removes the program but keeps the settings and logs in `%LOCALAPPDATA%\ConnectionWatcher` by default, so investigation records are not lost by accident. If you are sure you no longer need them, you can delete that folder manually.
