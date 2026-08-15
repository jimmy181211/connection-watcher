using ConnectionWatcher.App.Localization;

namespace ConnectionWatcher.App.UI;

internal sealed class DelayedSplashScreen : IDisposable
{
    private readonly StartupPresentation _presentation;
    private readonly string _language;
    private readonly TimeSpan _delay;
    private readonly ManualResetEventSlim _completed = new(false);
    private readonly object _gate = new();
    private Thread? _thread;
    private StartupSplashForm? _form;

    public DelayedSplashScreen(
        StartupPresentation presentation,
        string language,
        TimeSpan delay)
    {
        _presentation = presentation;
        _language = language;
        _delay = delay;
    }

    public void Start()
    {
        _thread = new Thread(Run)
        {
            IsBackground = true,
            Name = "SocketSight Startup"
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    public void Complete()
    {
        _completed.Set();
        StartupSplashForm? form;
        lock (_gate)
        {
            form = _form;
        }

        if (form is null || !form.IsHandleCreated || form.IsDisposed)
        {
            return;
        }

        try
        {
            form.BeginInvoke(new Action(form.Close));
        }
        catch (InvalidOperationException)
        {
            // The splash was already closing.
        }
    }

    private void Run()
    {
        if (_completed.Wait(_delay))
        {
            return;
        }

        using StartupSplashForm form = new(_presentation, _language);
        form.Shown += (_, _) =>
        {
            if (_completed.IsSet)
            {
                form.Close();
            }
        };
        lock (_gate)
        {
            _form = form;
        }

        if (!_completed.IsSet)
        {
            Application.Run(form);
        }

        lock (_gate)
        {
            _form = null;
        }
    }

    public void Dispose()
    {
        Complete();
        if (_thread is { IsAlive: true })
        {
            _thread.Join(TimeSpan.FromSeconds(2));
        }
        _completed.Dispose();
    }
}
