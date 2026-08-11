using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ConnectionWatcher.Core.Configuration;
using ConnectionWatcher.Core.Logging;
using ConnectionWatcher.Core.Models;
using ConnectionWatcher.Core.Monitoring;
using ConnectionWatcher.Core.Rules;

List<(string Name, Func<Task> Run)> tests =
[
    ("Port range parsing", TestPortRange),
    ("Exact connection rule matching", TestRuleMatching),
    ("Local listener matching", TestListenerMatching),
    ("Rule validation", TestValidation),
    ("Ongoing connection is logged once", TestOngoingConnectionDeduplication),
    ("Reconnect is logged after grace period", TestReconnect),
    ("Highest matching action wins", TestOverlappingRules),
    ("New rule can match an ongoing connection", TestNewRuleMatch),
    ("CSV event log round trip", TestCsvLog),
    ("CSV event log rotation is bounded", TestCsvRotation),
    ("Settings round trip", TestSettings),
    ("Windows TCP provider smoke test", TestWindowsProvider),
    ("Live local TCP monitoring end to end", TestLiveMonitoring)
];

int failures = 0;
foreach ((string name, Func<Task> run) in tests)
{
    try
    {
        await run();
        Console.WriteLine($"PASS  {name}");
    }
    catch (Exception ex)
    {
        failures++;
        Console.WriteLine($"FAIL  {name}: {ex.Message}");
    }
}

Console.WriteLine($"{tests.Count - failures}/{tests.Count} tests passed.");
return failures == 0 ? 0 : 1;

static Task TestPortRange()
{
    Assert(PortRange.TryParse("1433", false, out PortRange single));
    Assert(single.Contains(1433));
    Assert(!single.Contains(1434));
    Assert(PortRange.TryParse("1400-1500", false, out PortRange range));
    Assert(range.Contains(1433));
    Assert(!PortRange.TryParse("70000", false, out _));
    Assert(PortRange.TryParse(null, true, out PortRange any) && any.IsAny);
    return Task.CompletedTask;
}

static Task TestRuleMatching()
{
    MonitoringRule rule = NewRule(
        "UCSD",
        "103.1.40.235",
        new PortRange(1433, 1433),
        PortRange.Any,
        MatchAction.PopupAlert);
    Assert(RuleMatcher.Matches(rule, Connection("103.1.40.235", 1433)));
    Assert(!RuleMatcher.Matches(rule, Connection("103.1.40.236", 1433)));
    Assert(!RuleMatcher.Matches(rule, Connection("103.1.40.235", 443)));
    return Task.CompletedTask;
}

static Task TestListenerMatching()
{
    MonitoringRule rule = new()
    {
        Name = "Local SQL listener",
        Type = MonitoringRuleType.LocalListener,
        LocalPort = new PortRange(1433, 1433)
    };
    TcpConnectionInfo listener = new(
        IPAddress.Loopback,
        1433,
        IPAddress.Any,
        0,
        TcpState.Listen,
        42,
        "server",
        null);
    Assert(RuleMatcher.Matches(rule, listener));
    Assert(!RuleMatcher.Matches(rule, listener with { LocalPort = 1434 }));
    return Task.CompletedTask;
}

static Task TestValidation()
{
    MonitoringRule empty = new();
    IReadOnlyList<RuleValidationError> errors = RuleValidator.Validate(empty);
    Assert(errors.Contains(RuleValidationError.NameRequired));
    Assert(errors.Contains(RuleValidationError.AtLeastOneConditionRequired));

    MonitoringRule valid = NewRule(
        "Port 1433",
        null,
        new PortRange(1433, 1433),
        PortRange.Any,
        MatchAction.SilentLog);
    Assert(RuleValidator.Validate(valid).Count == 0);
    return Task.CompletedTask;
}

static Task TestOngoingConnectionDeduplication()
{
    ConnectionTracker tracker = new();
    MonitoringRule rule = NewRule(
        "UCSD",
        "103.1.40.235",
        new PortRange(1433, 1433),
        PortRange.Any,
        MatchAction.PopupAlert);
    TcpConnectionInfo connection = Connection("103.1.40.235", 1433);
    Assert(tracker.Process([connection], [rule], DateTimeOffset.Now).Count == 1);
    Assert(tracker.Process([connection], [rule], DateTimeOffset.Now).Count == 0);
    Assert(tracker.Process([connection], [rule], DateTimeOffset.Now).Count == 0);
    return Task.CompletedTask;
}

static Task TestReconnect()
{
    ConnectionTracker tracker = new();
    MonitoringRule rule = NewRule(
        "UCSD",
        "103.1.40.235",
        new PortRange(1433, 1433),
        PortRange.Any,
        MatchAction.PopupAlert);
    TcpConnectionInfo connection = Connection("103.1.40.235", 1433);
    tracker.Process([connection], [rule], DateTimeOffset.Now);
    tracker.Process([], [rule], DateTimeOffset.Now);
    tracker.Process([], [rule], DateTimeOffset.Now);
    Assert(tracker.Process([connection], [rule], DateTimeOffset.Now).Count == 1);
    return Task.CompletedTask;
}

static Task TestOverlappingRules()
{
    ConnectionTracker tracker = new();
    MonitoringRule broad = NewRule(
        "All 1433",
        null,
        new PortRange(1433, 1433),
        PortRange.Any,
        MatchAction.SilentLog);
    MonitoringRule urgent = NewRule(
        "UCSD",
        "103.1.40.235",
        new PortRange(1433, 1433),
        PortRange.Any,
        MatchAction.PopupAlert);
    IReadOnlyList<ConnectionEvent> events = tracker.Process(
        [Connection("103.1.40.235", 1433)],
        [broad, urgent],
        DateTimeOffset.Now);
    Assert(events.Count == 1);
    Assert(events[0].Action == MatchAction.PopupAlert);
    Assert(events[0].RuleNames.Count == 2);
    return Task.CompletedTask;
}

static Task TestNewRuleMatch()
{
    ConnectionTracker tracker = new();
    TcpConnectionInfo connection = Connection("103.1.40.235", 1433);
    MonitoringRule first = NewRule(
        "Broad",
        null,
        new PortRange(1433, 1433),
        PortRange.Any,
        MatchAction.SilentLog);
    MonitoringRule second = NewRule(
        "Exact",
        "103.1.40.235",
        new PortRange(1433, 1433),
        PortRange.Any,
        MatchAction.PopupAlert);
    tracker.Process([connection], [first], DateTimeOffset.Now);
    IReadOnlyList<ConnectionEvent> events = tracker.Process(
        [connection],
        [first, second],
        DateTimeOffset.Now);
    Assert(events.Count == 1);
    Assert(events[0].RuleNames.SequenceEqual(["Exact"]));
    return Task.CompletedTask;
}

static async Task TestCsvLog()
{
    string directory = Path.Combine(
        Path.GetTempPath(),
        "ConnectionWatcherTests",
        Guid.NewGuid().ToString("N"));
    try
    {
        CsvEventLogger logger = new(directory, maximumFileBytes: 1024, maximumFiles: 5);
        ConnectionEvent entry = ConnectionEvent.Create(
            DateTimeOffset.Now,
            Connection("103.1.40.235", 1433),
            [NewRule("Rule, with comma", "103.1.40.235", new PortRange(1433, 1433), PortRange.Any, MatchAction.PopupAlert)]);
        await logger.AppendAsync(entry);
        IReadOnlyList<ConnectionEvent> read = await logger.ReadRecentAsync();
        Assert(read.Count == 1);
        Assert(read[0].RemoteAddress == "103.1.40.235");
        Assert(read[0].RuleNames.Single() == "Rule, with comma");
    }
    finally
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}

static async Task TestCsvRotation()
{
    string directory = Path.Combine(
        Path.GetTempPath(),
        "ConnectionWatcherTests",
        Guid.NewGuid().ToString("N"));
    try
    {
        CsvEventLogger logger = new(directory, maximumFileBytes: 500, maximumFiles: 3);
        MonitoringRule rule = NewRule(
            "Rotation rule",
            "103.1.40.235",
            new PortRange(1433, 1433),
            PortRange.Any,
            MatchAction.SilentLog);
        for (int index = 0; index < 30; index++)
        {
            ConnectionEvent entry = ConnectionEvent.Create(
                DateTimeOffset.Now.AddSeconds(index),
                Connection("103.1.40.235", 1433) with
                {
                    ProcessName = "process-with-a-long-name-for-rotation-testing.exe"
                },
                [rule]);
            await logger.AppendAsync(entry);
        }

        Assert(Directory.GetFiles(directory, "events*.csv").Length <= 3);
    }
    finally
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}

static Task TestSettings()
{
    string directory = Path.Combine(
        Path.GetTempPath(),
        "ConnectionWatcherTests",
        Guid.NewGuid().ToString("N"));
    try
    {
        SettingsStore store = new(directory);
        AppSettings settings = new()
        {
            Language = "zh-CN",
            Rules =
            [
                NewRule("UCSD", "103.1.40.235", new PortRange(1433, 1433), PortRange.Any, MatchAction.PopupAlert)
            ]
        };
        store.Save(settings);
        AppSettings loaded = store.Load();
        Assert(loaded.Language == "zh-CN");
        Assert(loaded.Rules.Count == 1);
        Assert(loaded.Rules[0].RemotePort.Contains(1433));
    }
    finally
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    return Task.CompletedTask;
}

static Task TestWindowsProvider()
{
    WindowsTcpConnectionProvider provider = new();
    IReadOnlyList<TcpConnectionInfo> connections = provider.GetConnections();
    Assert(connections.All(connection => connection.LocalPort is >= 0 and <= 65535));
    return Task.CompletedTask;
}

static async Task TestLiveMonitoring()
{
    using TcpListener listener = new(IPAddress.Loopback, 0);
    listener.Start();
    int port = ((IPEndPoint)listener.LocalEndpoint).Port;
    using TcpClient client = new();
    Task<TcpClient> accept = listener.AcceptTcpClientAsync();
    await client.ConnectAsync(IPAddress.Loopback, port);
    using TcpClient accepted = await accept;

    MonitoringRule rule = NewRule(
        "Live loopback test",
        "127.0.0.1",
        new PortRange(port, port),
        PortRange.Any,
        MatchAction.SilentLog);
    MemoryLogger logger = new();
    await using MonitoringEngine engine = new(
        new WindowsTcpConnectionProvider(),
        logger,
        () => [rule]);
    TaskCompletionSource<ConnectionEvent> detected = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    engine.EventDetected += (_, entry) => detected.TrySetResult(entry);
    engine.Start();

    ConnectionEvent result = await detected.Task.WaitAsync(TimeSpan.FromSeconds(5));
    Assert(result.RemoteAddress == "127.0.0.1");
    Assert(result.RemotePort == port);
    await Task.Delay(1200);
    Assert(logger.Entries.Count == 1);
    await engine.StopAsync();
}

static MonitoringRule NewRule(
    string name,
    string? remoteIp,
    PortRange remotePort,
    PortRange localPort,
    MatchAction action)
{
    return new MonitoringRule
    {
        Name = name,
        RemoteIp = remoteIp,
        RemotePort = remotePort,
        LocalPort = localPort,
        Action = action,
        Enabled = true
    };
}

static TcpConnectionInfo Connection(string remoteIp, int remotePort)
{
    return new TcpConnectionInfo(
        IPAddress.Parse("172.20.10.2"),
        61659,
        IPAddress.Parse(remoteIp),
        remotePort,
        TcpState.Established,
        2480,
        "taskhostw",
        @"C:\Windows\System32\taskhostw.exe");
}

static void Assert(bool condition)
{
    if (!condition)
    {
        throw new InvalidOperationException("Assertion failed.");
    }
}

sealed class MemoryLogger : IEventLogger
{
    public List<ConnectionEvent> Entries { get; } = [];

    public Task AppendAsync(ConnectionEvent connectionEvent, CancellationToken cancellationToken = default)
    {
        lock (Entries)
        {
            Entries.Add(connectionEvent);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ConnectionEvent>> ReadRecentAsync(
        int maximumEntries = 2000,
        CancellationToken cancellationToken = default)
    {
        lock (Entries)
        {
            return Task.FromResult<IReadOnlyList<ConnectionEvent>>(
                Entries.TakeLast(maximumEntries).Reverse().ToArray());
        }
    }
}
