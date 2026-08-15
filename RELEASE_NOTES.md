# Release Notes

## v1.4.0

### Added

- A localized **Feedback** form that opens a pre-filled GitHub Issue for the user to review and submit. Logs and connection records are not attached by default.
- The latest project overview and user guide are included for all supported interface languages.

### Notes

- Feedback is sent only after the user reviews the pre-filled issue page and submits it on GitHub. The app does not use a GitHub account token or upload logs automatically.

## v1.3.0

### Added

- A persistent **Clear display** action for the Event Log. It hides earlier events from the interface without deleting CSV records.
- Configurable TCP check intervals from **0.5 to 10.0 seconds**, in 0.5-second steps.
- Deeper application context for newly matched connections: the owner reported by Windows, file metadata, up to three parent/host processes, and related Windows services when available.
- A manual **Check for updates** action that reads the latest public GitHub release only when requested.

### Changed

- The Event Log now labels its compact process column as **Application** and keeps detailed attribution in the double-click event view.
- The alert-sound test button now sits beside the volume control for a clearer Settings layout.
- A connection is considered ended after it has been absent for two seconds of wall-clock time, independent of the selected check interval.
- End times use the last moment the connection was actually observed.
- CSV logs include the new process-context fields while remaining compatible with logs from earlier versions.
- Maintainers can now build, test, package, collect current documents, and generate checksums with one release script.

### Notes

- Process and parent information is evidence supplied or inferred from Windows at the time of detection. It may not identify the application that ultimately caused an already-running browser, proxy, VPN, or embedded web component to connect.
- Update checking never downloads or installs software automatically and does not upload rules or logs.
- UDP monitoring remains outside this release because reliable remote-endpoint and application attribution would require a substantially different privileged tracing design.
