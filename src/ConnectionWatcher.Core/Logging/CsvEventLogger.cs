using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Logging;

public sealed class CsvEventLogger : IEventLogger
{
    private const string Header =
        "Record Type/记录类型,First Seen/首次发现,Last Seen/最后发现," +
        "Ended At/结束时间,Rules/规则,Action/操作,TCP State/TCP状态," +
        "Local IP/本地IP,Local Port/本地端口,Remote IP/远程IP," +
        "Remote Port/远程端口,PID,Connection Owner/连接所属进程,Path/路径," +
        "Product/产品名称,Company/公司,File Description/文件说明," +
        "Parent Processes/父级进程,Related Services/相关服务,Event ID/事件ID";

    private readonly SemaphoreSlim _gate = new(1, 1);
    private long _maximumTotalBytes;
    private readonly int _maximumFiles;

    public CsvEventLogger(
        string logDirectory,
        long maximumFileBytes = 5 * 1024 * 1024,
        int maximumFiles = 5)
    {
        LogDirectory = logDirectory;
        _maximumFiles = maximumFiles;
        _maximumTotalBytes = checked(maximumFileBytes * maximumFiles);
    }

    public string LogDirectory { get; }
    public string CurrentLogPath => Path.Combine(LogDirectory, "events.csv");

    public async Task UpdateMaximumTotalBytesAsync(
        long maximumTotalBytes,
        CancellationToken cancellationToken = default)
    {
        if (maximumTotalBytes < _maximumFiles)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumTotalBytes));
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Interlocked.Exchange(ref _maximumTotalBytes, maximumTotalBytes);
            if (Directory.Exists(LogDirectory))
            {
                RotateIfNeeded(0);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task AppendAsync(
        ConnectionEvent connectionEvent,
        CancellationToken cancellationToken = default)
    {
        await AppendRecordAsync("Start", connectionEvent, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AppendCompletionAsync(
        ConnectionEvent connectionEvent,
        CancellationToken cancellationToken = default)
    {
        await AppendRecordAsync("End", connectionEvent, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task AppendRecordAsync(
        string recordType,
        ConnectionEvent connectionEvent,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(LogDirectory);
            EnsureCurrentSchema();
            string line = Format(recordType, connectionEvent);
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

            Dictionary<Guid, ConnectionEvent> entries = [];
            foreach (string path in Directory.GetFiles(LogDirectory, "events*.csv")
                         .OrderBy(File.GetLastWriteTimeUtc))
            {
                string[] lines = await File.ReadAllLinesAsync(path, cancellationToken)
                    .ConfigureAwait(false);
                foreach (string line in lines.Skip(1))
                {
                    if (TryParse(line, out ConnectionEvent? entry) && entry is not null)
                    {
                        entries[entry.EventId] = entry;
                    }
                }
            }

            return entries.Values
                .OrderByDescending(entry => entry.DetectedAt)
                .Take(maximumEntries)
                .ToArray();
        }
        finally
        {
            _gate.Release();
        }
    }

    private void RotateIfNeeded(long additionalBytes)
    {
        long maximumTotalBytes = Interlocked.Read(ref _maximumTotalBytes);
        long maximumFileBytes = Math.Max(1, maximumTotalBytes / _maximumFiles);
        bool currentWillBeCreated = !File.Exists(CurrentLogPath);
        if (File.Exists(CurrentLogPath) &&
            new FileInfo(CurrentLogPath).Length + additionalBytes > maximumFileBytes)
        {
            ArchiveCurrentLog();
            currentWillBeCreated = true;
        }

        FileInfo[] files = new DirectoryInfo(LogDirectory)
            .GetFiles("events*.csv")
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToArray();
        int filesToKeep = currentWillBeCreated ? _maximumFiles - 1 : _maximumFiles;
        foreach (FileInfo obsolete in files.Skip(filesToKeep))
        {
            obsolete.Delete();
        }

        files = new DirectoryInfo(LogDirectory)
            .GetFiles("events*.csv")
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToArray();
        long totalBytes = files.Sum(file => file.Length);
        foreach (FileInfo obsolete in files.Reverse())
        {
            if (totalBytes <= maximumTotalBytes ||
                obsolete.FullName.Equals(CurrentLogPath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            totalBytes -= obsolete.Length;
            obsolete.Delete();
        }
    }

    private void EnsureCurrentSchema()
    {
        if (!File.Exists(CurrentLogPath) || new FileInfo(CurrentLogPath).Length == 0)
        {
            return;
        }

        string? currentHeader = File.ReadLines(CurrentLogPath).FirstOrDefault();
        if (!string.Equals(currentHeader, Header, StringComparison.Ordinal))
        {
            ArchiveCurrentLog();
        }
    }

    private void ArchiveCurrentLog()
    {
        string archiveName = $"events-{DateTime.Now:yyyyMMdd-HHmmss-fff}";
        string archivedPath = Path.Combine(LogDirectory, archiveName + ".csv");
        int suffix = 1;
        while (File.Exists(archivedPath))
        {
            archivedPath = Path.Combine(
                LogDirectory,
                $"{archiveName}-{suffix++}.csv");
        }

        File.Move(CurrentLogPath, archivedPath);
    }

    private static string Format(string recordType, ConnectionEvent entry)
    {
        string[] fields =
        [
            recordType,
            FormatTimestamp(entry.DetectedAt),
            FormatTimestamp(entry.LastSeenAt),
            entry.EndedAt is null ? string.Empty : FormatTimestamp(entry.EndedAt.Value),
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
            entry.ProcessProductName ?? string.Empty,
            entry.ProcessCompanyName ?? string.Empty,
            entry.ProcessFileDescription ?? string.Empty,
            JsonSerializer.Serialize(entry.ParentProcesses),
            JsonSerializer.Serialize(entry.RelatedServices),
            entry.EventId.ToString()
        ];
        return string.Join(',', fields.Select(Escape));
    }

    private static string FormatTimestamp(DateTimeOffset value) =>
        value.ToLocalTime().ToString(
            "yyyy-MM-dd HH:mm:ss zzz",
            CultureInfo.InvariantCulture);

    private static string Escape(string value)
    {
        string flattened = value.Replace('\r', ' ').Replace('\n', ' ');
        return '"' + flattened.Replace("\"", "\"\"") + '"';
    }

    private static bool TryParse(string line, out ConnectionEvent? entry)
    {
        entry = null;
        List<string> fields = ParseFields(line);
        if (fields.Count == 12)
        {
            return TryParseLegacy(fields, out entry);
        }

        if (fields.Count == 15)
        {
            return TryParseVersionTwo(fields, out entry);
        }

        DateTimeOffset parsedEndedAt = default;
        if (fields.Count != 20 ||
            (fields[0] != "Start" && fields[0] != "End") ||
            !DateTimeOffset.TryParse(fields[1], out DateTimeOffset detectedAt) ||
            !DateTimeOffset.TryParse(fields[2], out DateTimeOffset lastSeenAt) ||
            (!string.IsNullOrWhiteSpace(fields[3]) &&
             !DateTimeOffset.TryParse(fields[3], out parsedEndedAt)) ||
            !Enum.TryParse(fields[5], out MatchAction action) ||
            !Enum.TryParse(fields[6], out TcpState state) ||
            !int.TryParse(fields[8], out int localPort) ||
            !int.TryParse(fields[10], out int remotePort) ||
            !int.TryParse(fields[11], out int processId) ||
            !Guid.TryParse(fields[19], out Guid eventId))
        {
            return false;
        }

        DateTimeOffset? endedAt = string.IsNullOrWhiteSpace(fields[3])
            ? null
            : parsedEndedAt;

        entry = new ConnectionEvent
        {
            EventId = eventId,
            DetectedAt = detectedAt,
            LastSeenAt = lastSeenAt,
            EndedAt = endedAt,
            RuleNames = fields[4].Split(" | ", StringSplitOptions.RemoveEmptyEntries),
            Action = action,
            State = state,
            LocalAddress = fields[7],
            LocalPort = localPort,
            RemoteAddress = fields[9],
            RemotePort = remotePort,
            ProcessId = processId,
            ProcessName = fields[12],
            ProcessPath = EmptyToNull(fields[13]),
            ProcessProductName = EmptyToNull(fields[14]),
            ProcessCompanyName = EmptyToNull(fields[15]),
            ProcessFileDescription = EmptyToNull(fields[16]),
            ParentProcesses = DeserializeList<ProcessSnapshot>(fields[17]),
            RelatedServices = DeserializeList<WindowsServiceSnapshot>(fields[18])
        };
        return true;
    }

    private static bool TryParseVersionTwo(
        IReadOnlyList<string> fields,
        out ConnectionEvent? entry)
    {
        entry = null;
        DateTimeOffset parsedEndedAt = default;
        if ((fields[0] != "Start" && fields[0] != "End") ||
            !DateTimeOffset.TryParse(fields[1], out DateTimeOffset detectedAt) ||
            !DateTimeOffset.TryParse(fields[2], out DateTimeOffset lastSeenAt) ||
            (!string.IsNullOrWhiteSpace(fields[3]) &&
             !DateTimeOffset.TryParse(fields[3], out parsedEndedAt)) ||
            !Enum.TryParse(fields[5], out MatchAction action) ||
            !Enum.TryParse(fields[6], out TcpState state) ||
            !int.TryParse(fields[8], out int localPort) ||
            !int.TryParse(fields[10], out int remotePort) ||
            !int.TryParse(fields[11], out int processId) ||
            !Guid.TryParse(fields[14], out Guid eventId))
        {
            return false;
        }

        entry = new ConnectionEvent
        {
            EventId = eventId,
            DetectedAt = detectedAt,
            LastSeenAt = lastSeenAt,
            EndedAt = string.IsNullOrWhiteSpace(fields[3]) ? null : parsedEndedAt,
            RuleNames = fields[4].Split(" | ", StringSplitOptions.RemoveEmptyEntries),
            Action = action,
            State = state,
            LocalAddress = fields[7],
            LocalPort = localPort,
            RemoteAddress = fields[9],
            RemotePort = remotePort,
            ProcessId = processId,
            ProcessName = fields[12],
            ProcessPath = EmptyToNull(fields[13])
        };
        return true;
    }

    private static bool TryParseLegacy(
        IReadOnlyList<string> fields,
        out ConnectionEvent? entry)
    {
        entry = null;
        if (!DateTimeOffset.TryParse(fields[0], out DateTimeOffset detectedAt) ||
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
            LastSeenAt = detectedAt,
            EndedAt = detectedAt,
            DurationKnown = false,
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

    private static IReadOnlyList<T> DeserializeList<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<T>();
        }

        try
        {
            return JsonSerializer.Deserialize<T[]>(json) ?? Array.Empty<T>();
        }
        catch (JsonException)
        {
            return Array.Empty<T>();
        }
    }

    private static string? EmptyToNull(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
