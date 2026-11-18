# SocketSight Privacy Policy

**Effective and last updated: September 6, 2026**

SocketSight is designed to work locally on Windows. This policy explains what the app observes, what it stores on your computer, and the limited situations in which it connects to GitHub.

## 1. No account, advertising, or analytics

SocketSight does not require an account. It does not include advertising trackers, behavioral analytics, or cloud telemetry, and it does not sell personal information.

## 2. Information processed locally

When monitoring is enabled, SocketSight reads the Windows TCP connection table and processes only connections that match rules you created. Depending on what Windows makes available, a local event may include:

- local and remote IP addresses and ports;
- TCP state and observed duration;
- process name, process identifier, and executable path;
- file product, company, and description metadata;
- parent-process or Windows-service context; and
- the monitoring rule that matched.

SocketSight does not inspect packet contents. Rules, settings, matched events, and CSV logs remain on your computer and are not uploaded by the app.

## 3. Local storage and deletion

Settings and monitoring rules are stored in the current Windows user's local application-data area. Event records are stored in local CSV files. The configured log limit controls automatic rotation across five log files; older records are removed first when the limit is reached.

You can open the log folder from SocketSight and delete exported or stored CSV files with Windows. Uninstalling the app may not remove user-created logs or settings, so remove those local files separately if you no longer want them.

## 4. Optional network connections

SocketSight contacts GitHub only when you deliberately use one of these features:

- **Check for updates:** the app requests public release information from the GitHub API. GitHub receives ordinary request information such as your IP address, request time, and the SocketSight application user-agent.
- **Send feedback:** the app opens a GitHub Issue page in your browser with text you entered. If you choose to include basic diagnostics, the draft also contains the SocketSight version, Windows version, and interface language. Logs and network connection records are not attached. You review and submit the issue yourself.

GitHub processes those interactions under its own privacy terms. Do not place passwords, private network records, confidential file paths, or other sensitive information in a public GitHub Issue.

## 5. Windows integration

If you enable launch at Windows sign-in, SocketSight creates local startup configuration for the current Windows user. This preference is not transmitted to us.

## 6. Children

SocketSight is a general-purpose technical utility and is not directed to children under 13. We do not knowingly collect personal information from children.

## 7. Changes and contact

We may update this policy when SocketSight's behavior changes. The date above identifies the current version. Questions or privacy concerns may be raised through the project's [GitHub Issues](https://github.com/jimmy181211/connection-watcher/issues). Do not include sensitive information in a public issue.
