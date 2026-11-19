using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;

namespace BD.WTTS.UI.Views.Controls;

public partial class ProxyLog : UserControl
{
    CancellationTokenSource? cancellation;
    IDisposable? statusSubscription;
    bool running;

    public ProxyLog()
    {
        InitializeComponent();

        var logTextbox = this.FindControl<TextEditor>("LogTextbox")!;

        LogSyntaxHighlighting.Apply(logTextbox);

        logTextbox.Document.UndoStack.SizeLimit = 0;
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
        UpdateProxyStatus(false);
        base.OnDetachedFromVisualTree(e);
    }

    void UpdateProxyStatus(bool value)
    {
        running = value;
        if (!running)
        {
            cancellation?.Cancel();
        }
        else if (cancellation == null)
        {
            var source = cancellation = new CancellationTokenSource();
            _ = RefreshUntilStoppedAsync(source);
        }
    }

    async Task RefreshUntilStoppedAsync(CancellationTokenSource source)
    {
        try
        {
            await Task.Run(() => FlushLogsAsync(source.Token)).ConfigureAwait(false);
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                source.Dispose();
                cancellation = null;
                // A detached reader may still be inside synchronous IPC when the view reopens.
                if (running)
                    UpdateProxyStatus(true);
            });
        }
    }

    private void LogTextbox_TextChanged(object? sender, EventArgs e)
    {
        LogTextbox.ScrollToLine(LogTextbox.Document.LineCount);
    }

    async Task FlushLogsAsync(CancellationToken token)
    {
        string? previous = null;
        while (!token.IsCancellationRequested)
        {
            try
            {
                var logtext = IReverseProxyService.Constants.Instance.GetLogAllMessage() ?? string.Empty;
                if (!string.Equals(logtext, previous, StringComparison.Ordinal))
                {
                    var append = previous != null && logtext.Length > previous.Length &&
                        logtext.StartsWith(previous, StringComparison.Ordinal);
                    var update = append ? logtext[previous!.Length..] : logtext;
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (token.IsCancellationRequested)
                            return;

                        if (append)
                            LogTextbox.AppendText(update);
                        else
                            LogTextbox.Text = update;
                    });
                    previous = logtext;
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
