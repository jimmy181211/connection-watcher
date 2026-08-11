# TCP Connection Watcher User Guide

## Main purpose

Simply put, this tool helps you **watch an IP address or port that you choose**. It can:

- Automatically record when a connection appears
- Record the local and remote IP addresses and ports
- Record the related program, PID, and executable path whenever available
- Log silently, show a tray notice, or display a pop-up alert according to your settings
- Save records for later review or for sharing with cybersecurity staff
- Help you confirm whether a new connection to the same target appears later

## How the tool works

First, create a rule to tell the app which IP address or port to watch. Then enable the rule and start monitoring. The app checks the Windows TCP connection list once per second. It processes only connections that match an enabled rule. Other normal connections do not create records or alerts.

When a connection matches a rule, the app follows your selected action:

- **Log silently:** Writes the event to the CSV log without changing the tray icon or showing an unread count.
- **Tray notice and log:** Does not show a pop-up. The tray icon changes to a warning state, and opening the Event log page clears the notice.
- **Pop up alert and log:** Shows a window as soon as the first match appears. While the window is open, later matches update the same window. After it is closed, the rule's repeat interval controls when another alert can appear.

#### *Important note:*

1. A rule match means only that a connection you chose to watch has appeared. It does not prove that the computer has a virus.
2. This tool **only records connections and shows alerts**. Decisions about further security action should also consider antivirus scan results and advice from qualified professionals.

## First run

1. Choose Chinese or English.
2. Open **Monitoring rules**.
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

Each new matching connection adds one row. If the same connection stays open for several hours, it is not written again every second. A new record is created only after the connection disappears for two checks and then appears again.

The total log limit is 25 MB by default and can be changed to 5–500 MB in **Settings**. The app uses up to five log files and automatically removes the oldest records when the selected limit is reached.

## Help center

In **Settings**, select **Open help center** to read the project overview and user guide inside the app.

## Important limitations

1. The app checks once per second, so it may miss a connection that lasts for less than one second.
2. Version 1 **monitors TCP only**. It does not monitor UDP.
3. The Windows TCP connection table does not provide a completely reliable connection-initiator field, so the app cannot determine which side started a connection.
4. Windows permissions may prevent the app from reading the executable path of some system or protected processes. The PID and any available process name are still recorded.
5. The app does not monitor while it is closed, monitoring is stopped, or the computer is asleep.
6. The app only records connections and shows alerts. It does not close programs, change firewall settings, or block IP addresses.

## Privacy and permissions

1. Administrator rights are not required.
2. No login, username, password, or email address is required.
3. The app does not connect to a developer server or upload logs.
4. It does not read packet contents.
5. Settings are stored in `%LOCALAPPDATA%\ConnectionWatcher\config.json`.

## Uninstall

You can remove the installed version through **Installed apps** in Windows. Uninstalling removes the program but keeps the settings and logs in `%LOCALAPPDATA%\ConnectionWatcher` by default, so investigation records are not lost by accident. If you are sure you no longer need them, you can delete that folder manually.
