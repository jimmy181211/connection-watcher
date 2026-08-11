using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Logging;

public sealed class CsvEventLogger : IEventLogger
{
    private const string Header =
        "Time/时间,Rules/规则,Action/操作,TCP State/TCP状态," +
        "Local IP/本地IP,Local Port/本地端口,Remote IP/远程IP," +
        "Remote Port/远程端口,PID,Program/程序,Path/路径,Event ID/事件ID";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly long _maximumFileBytes;
    private readonly int _maximumFiles;

    public CsvEventLogger(
        string logDirectory,
        long maximumFileBytes = 5 * 1024 * 1024,
        int maximumFiles = 5)
    {
        LogDirectory = logDirectory;
        _maximumFileBytes = maximumFileBytes;
        _maximumFiles = maximumFiles;
    }

    public string LogDirectory { get; }
    public string CurrentLogPath => Path.Combine(LogDirectory, "events.csv");

    public async Task AppendAsync(
        ConnectionEvent connectionEvent,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(LogDirectory);
            string line = Format(connectionEvent);
            long additionalBytes = Encoding.UTF8.GetByteCount(line + Environment.NewLine);
            RotateIfNeeded(additionalBytes);

            bool writeHeader = !File.Exists(CurrentLogPath) ||
                new FileInfo(CurrentLogPath).Length == 0;
            await using FileStream stream = new(
                CurrentLogPath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read);
            await using StreamWriter writer = new(stream, new UTF8Encoding(true));
            if (writeHeader)
            {
                await writer.WriteLineAsync(Header.AsMemory(), cancellationToken)
                    .ConfigureAwait(false);
            }

            await writer.WriteLineAsync(line.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<ConnectionEvent>> ReadRecentAsync(
        int maximumEntries = 2000,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!Directory.Exists(LogDirectory))
            {
                return Array.Empty<ConnectionEvent>();
            }

            List<ConnectionEvent> entries = [];
            foreach (string path in Directory.GetFiles(LogDirectory, "events*.csv")
                         .OrderBy(File.GetLastWriteTimeUtc))
            {
                string[] lines = await File.ReadAllLinesAsync(path, cancellationToken)
                    .ConfigureAwait(false);
                foreach (string line in lines.Skip(1))
                {
                    if (TryParse(line, out ConnectionEvent? entry) && entry is not null)
                    {
                        entries.Add(entry);
                    }
                }
            }

            return entries.TakeLast(maximumEntries).Reverse().ToArray();
        }
        finally
        {
            _gate.Release();
        }
    }

    private void RotateIfNeeded(long additionalBytes)
    {
        bool currentWillBeCreated = false;
        if (File.Exists(CurrentLogPath) &&
            new FileInfo(CurrentLogPath).Length + additionalBytes > _maximumFileBytes)
        {
            string archivedPath = Path.Combine(
                LogDirectory,
                $"events-{DateTime.Now:yyyyMMdd-HHmmss-fff}.csv");
            File.Move(CurrentLogPath, archivedPath);
            currentWillBeCreated = true;
        }

        string[] files = Directory.GetFiles(LogDirectory, "events*.csv")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .ToArray();
        int filesToKeep = currentWillBeCreated ? _maximumFiles - 1 : _maximumFiles;
        foreach (string obsolete in files.Skip(filesToKeep))
        {
            File.Delete(obsolete);
        }
    }

    private static string Format(ConnectionEvent entry)
    {
        string[] fields =
        [
            entry.DetectedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture),
            string.Join(" | ", entry.RuleNames),
            entry.Action.ToString(),
            entry.State.ToString(),
            entry.LocalAddress,
            entry.LocalPort.ToString(CultureInfo.InvariantCulture),
            entry.RemoteAddress,
            entry.RemotePort.ToString(CultureInfo.InvariantCulture),
            entry.ProcessId.ToString(CultureInfo.InvariantCulture),
            entry.ProcessName,
            entry.ProcessPath ?? string.Empty,
            entry.EventId.ToString()
        ];
        return string.Join(',', fields.Select(Escape));
    }

    private static string Escape(string value)
    {
        string flattened = value.Replace('\r', ' ').Replace('\n', ' ');
        return '"' + flattened.Replace("\"", "\"\"") + '"';
    }

    private static bool TryParse(string line, out ConnectionEvent? entry)
    {
        entry = null;
        List<string> fields = ParseFields(line);
        if (fields.Count != 12 ||
            !DateTimeOffset.TryParse(fields[0], out DateTimeOffset detectedAt) ||
            !Enum.TryParse(fields[2], out MatchAction action) ||
            !Enum.TryParse(fields[3], out TcpState state) ||
            !int.TryParse(fields[5], out int localPort) ||
            !int.TryParse(fields[7], out int remotePort) ||
            !int.TryParse(fields[8], out int processId) ||
            !Guid.TryParse(fields[11], out Guid eventId))
        {
            return false;
        }

        entry = new ConnectionEvent
        {
            EventId = eventId,
            DetectedAt = detectedAt,
            RuleNames = fields[1].Split(" | ", StringSplitOptions.RemoveEmptyEntries),
            Action = action,
            State = state,
            LocalAddress = fields[4],
            LocalPort = localPort,
            RemoteAddress = fields[6],
            RemotePort = remotePort,
            ProcessId = processId,
            ProcessName = fields[9],
            ProcessPath = string.IsNullOrWhiteSpace(fields[10]) ? null : fields[10]
        };
        return true;
    }

    private static List<string> ParseFields(string line)
    {
        List<string> fields = [];
        StringBuilder current = new();
        bool quoted = false;
        for (int index = 0; index < line.Length; index++)
        {
            char character = line[index];
            if (character == '"')
            {
                if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (character == ',' && !quoted)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(character);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }
}
