using ConnectionWatcher.Core.Logging;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Monitoring;

public sealed class MonitoringEngine : IAsyncDisposable
{
    private readonly ITcpConnectionProvider _provider;
    private readonly IEventLogger _logger;
    private readonly Func<IReadOnlyList<MonitoringRule>> _rulesProvider;
    private readonly ConnectionTracker _tracker = new();
    private CancellationTokenSource? _cancellation;
    private Task? _worker;

    public MonitoringEngine(
        ITcpConnectionProvider provider,
        IEventLogger logger,
        Func<IReadOnlyList<MonitoringRule>> rulesProvider)
    {
        _provider = provider;
        _logger = logger;
        _rulesProvider = rulesProvider;
    }

    public event EventHandler<ConnectionEvent>? EventDetected;
    public event EventHandler<Exception>? MonitoringError;
    public event EventHandler? MonitoringRecovered;

    public bool IsRunning => _worker is { IsCompleted: false };
    public TimeSpan Interval { get; } = TimeSpan.FromSeconds(1);

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
            _cancellation.Dispose();
            _cancellation = null;
            _worker = null;
            _tracker.Reset();
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(Interval);
        bool recoveringFromError = false;
        do
        {
            try
            {
                IReadOnlyList<TcpConnectionInfo> connections = _provider.GetConnections();
                IReadOnlyList<ConnectionEvent> events = _tracker.Process(
                    connections,
                    _rulesProvider(),
                    DateTimeOffset.Now);
                foreach (ConnectionEvent connectionEvent in events)
                {
                    await _logger.AppendAsync(connectionEvent, cancellationToken)
                        .ConfigureAwait(false);
                    EventDetected?.Invoke(this, connectionEvent);
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
        while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false));
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }
}
