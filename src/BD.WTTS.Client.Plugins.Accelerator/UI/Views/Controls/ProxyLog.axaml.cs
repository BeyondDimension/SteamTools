using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;

namespace BD.WTTS.UI.Views.Controls;

public partial class ProxyLog : UserControl
{
    CancellationTokenSource? cancellation;
    Task? refreshTask;
    IDisposable? statusSubscription;

    public ProxyLog()
    {
        InitializeComponent();

        var logTextbox = this.FindControl<TextEditor>("LogTextbox")!;

        LogSyntaxHighlighting.Apply(logTextbox);

        logTextbox.TextChanged += LogTextbox_TextChanged;

    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        statusSubscription = ProxyService.Current.WhenAnyValue(x => x.ProxyStatus)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(UpdateProxyStatus);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        statusSubscription?.Dispose();
        statusSubscription = null;
        StopRefreshing();
        base.OnDetachedFromVisualTree(e);
    }

    void UpdateProxyStatus(bool running)
    {
        if (!running)
        {
            StopRefreshing();
        }
        else if (cancellation == null)
        {
            var source = cancellation = new CancellationTokenSource();
            var token = source.Token;
            refreshTask = Task.Run(() => FlushLogsAsync(token));
        }
    }

    void StopRefreshing()
    {
        var source = cancellation;
        var task = refreshTask;
        cancellation = null;
        refreshTask = null;
        if (source == null)
            return;

        source.Cancel();
        _ = task!.ContinueWith(static (_, state) => ((CancellationTokenSource)state!).Dispose(),
            source, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private void LogTextbox_TextChanged(object? sender, EventArgs e)
    {
        LogTextbox.ScrollToLine(LogTextbox.Document.LineCount);
    }

    async Task FlushLogsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var logtext = IReverseProxyService.Constants.Instance.GetLogAllMessage();
                if (!string.IsNullOrEmpty(logtext))
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (!token.IsCancellationRequested && LogTextbox.Text.Length != logtext.Length)
                            LogTextbox.Text = logtext;
                    });
                }
            }
            catch
            {
            }

            try
            {
                await Task.Delay(1000, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                break;
            }
        }
    }
}