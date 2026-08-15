using ConnectionWatcher.Core.Logging;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Monitoring;

public sealed class MonitoringEngine : IAsyncDisposable
{
    private readonly ITcpConnectionProvider _provider;
    private readonly IEventLogger _logger;
    private readonly Func<IReadOnlyList<MonitoringRule>> _rulesProvider;
    private readonly IProcessContextProvider? _processContextProvider;
    private readonly ConnectionTracker _tracker = new();
    private readonly object _intervalGate = new();
    private readonly List<CancellationTokenSource> _retiredIntervalSignals = [];
    private CancellationTokenSource _intervalChanged = new();
    private TimeSpan _interval;
    private CancellationTokenSource? _cancellation;
    private Task? _worker;

    public MonitoringEngine(
        ITcpConnectionProvider provider,
        IEventLogger logger,
        Func<IReadOnlyList<MonitoringRule>> rulesProvider,
        IProcessContextProvider? processContextProvider = null,
        TimeSpan? interval = null)
    {
        _provider = provider;
        _logger = logger;
        _rulesProvider = rulesProvider;
        _processContextProvider = processContextProvider;
        _interval = interval ?? TimeSpan.FromSeconds(1);
    }

    public event EventHandler<ConnectionEvent>? EventDetected;
    public event EventHandler<ConnectionEvent>? EventCompleted;
    public event EventHandler<Exception>? MonitoringError;
    public event EventHandler? MonitoringRecovered;

    public bool IsRunning => _worker is { IsCompleted: false };
    public TimeSpan Interval
    {
        get
        {
            lock (_intervalGate)
            {
                return _interval;
            }
        }
    }

    public void UpdateInterval(TimeSpan interval)
    {
        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval));
        }

        CancellationTokenSource changed;
        lock (_intervalGate)
        {
            if (_interval == interval)
            {
                return;
            }

            _interval = interval;
            changed = _intervalChanged;
            _retiredIntervalSignals.Add(changed);
            _intervalChanged = new CancellationTokenSource();
        }

        changed.Cancel();
    }

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        if (!_rulesProvider().Any(rule => rule.Enabled))
        {
            throw new InvalidOperationException("At least one enabled rule is required.");
        }

        _tracker.Reset();
        _cancellation = new CancellationTokenSource();
        _worker = Task.Run(() => RunAsync(_cancellation.Token));
    }

    public async Task StopAsync()
    {
        if (_cancellation is null || _worker is null)
        {
            return;
        }

        _cancellation.Cancel();
        try
        {
            await _worker.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            try
            {
                foreach (ConnectionEvent connectionEvent in _tracker.CompleteAll())
                {
                    try
                    {
                        await _logger.AppendCompletionAsync(connectionEvent)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                    {
                        MonitoringError?.Invoke(this, ex);
                    }

                    EventCompleted?.Invoke(this, connectionEvent);
                }
            }
            finally
            {
                _cancellation.Dispose();
                _cancellation = null;
                _worker = null;
                _tracker.Reset();
            }
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        bool recoveringFromError = false;
        do
        {
            try
            {
                IReadOnlyList<TcpConnectionInfo> connections = _provider.GetConnections();
                ConnectionTrackingResult result = _tracker.Process(
                    connections,
                    _rulesProvider(),
                    DateTimeOffset.Now);
                foreach (ConnectionEvent connectionEvent in result.DetectedEvents)
                {
                    if (_processContextProvider is not null)
                    {
                        try
                        {
                            ProcessContext context = _processContextProvider.GetContext(
                                connectionEvent.ProcessId,
                                connectionEvent.ProcessName,
                                connectionEvent.ProcessPath);
                            connectionEvent.ApplyProcessContext(context);
                        }
                        catch
                        {
                            // Attribution is best-effort and must never stop monitoring.
                        }
                    }

                    await _logger.AppendAsync(connectionEvent, cancellationToken)
                        .ConfigureAwait(false);
                    EventDetected?.Invoke(this, connectionEvent);
                }
                foreach (ConnectionEvent connectionEvent in result.CompletedEvents)
                {
                    await _logger.AppendCompletionAsync(connectionEvent, cancellationToken)
                        .ConfigureAwait(false);
                    EventCompleted?.Invoke(this, connectionEvent);
                }

                if (recoveringFromError)
                {
                    recoveringFromError = false;
                    MonitoringRecovered?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                recoveringFromError = true;
                MonitoringError?.Invoke(this, ex);
            }
        }
        while (await WaitForNextPollAsync(cancellationToken).ConfigureAwait(false));
    }

    private async Task<bool> WaitForNextPollAsync(CancellationToken cancellationToken)
    {
        TimeSpan interval;
        CancellationToken intervalChanged;
        lock (_intervalGate)
        {
            interval = _interval;
            intervalChanged = _intervalChanged.Token;
        }

        using CancellationTokenSource waitCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                intervalChanged);
        try
        {
            await Task.Delay(interval, waitCancellation.Token).ConfigureAwait(false);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (OperationCanceledException) when (intervalChanged.IsCancellationRequested)
        {
            return true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        lock (_intervalGate)
        {
            _intervalChanged.Dispose();
            foreach (CancellationTokenSource signal in _retiredIntervalSignals)
            {
                signal.Dispose();
            }
            _retiredIntervalSignals.Clear();
        }
    }
}
