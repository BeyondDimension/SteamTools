using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using dotnetCampus.Ipc.Exceptions;

namespace BD.WTTS.UI.Views.Controls;

public partial class ProxyLog : UserControl
{
    CancellationTokenSource? cancellation;
    IDisposable? statusSubscription;
    bool running;
    VisualVisibilitySubscription? visibility;
    (Guid StreamId, long Offset) cursor;

    public ProxyLog()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        visibility = new(this, UpdatePolling);
        statusSubscription = ProxyService.Current.ObserveValue(nameof(ProxyService.ProxyStatus), static x => x.ProxyStatus)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(UpdateProxyStatus);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        visibility?.Dispose();
        visibility = null;
        statusSubscription?.Dispose();
        statusSubscription = null;
        UpdateProxyStatus(false);
        base.OnDetachedFromVisualTree(e);
    }

    void UpdateProxyStatus(bool value)
    {
        running = value;
        UpdatePolling();
        if (!value)
        {
            cursor = default;
            ConsoleView.ResetLog();
        }
    }

    void UpdatePolling()
    {
        if (visibility == null || !IsEffectivelyVisible || !running)
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
            var start = cursor;
            await Task.Run(() => FlushLogsAsync(start, source.Token)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(ProxyLog), ex, "Live accelerator log reader stopped unexpectedly.");
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var wasCanceled = source.IsCancellationRequested;
                source.Dispose();
                cancellation = null;
                // Visibility can resume while the canceled reader still has an outstanding request.
                if (wasCanceled) UpdatePolling();
            });
        }
    }

    async Task FlushLogsAsync((Guid StreamId, long Offset) position, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var update = await IReverseProxyService.Constants.Instance.GetLogUpdateAsync(position.StreamId, position.Offset).ConfigureAwait(false);
                if (update != null && !token.IsCancellationRequested)
                {
                    var text = Encoding.UTF8.GetString(update.Utf8);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (token.IsCancellationRequested) return;
                        ConsoleView.WriteText(text, replace: update.Reset);
                        cursor = (update.StreamId, update.End);
                    }, DispatcherPriority.Background);
                    position = (update.StreamId, update.End);
                }
            }
            catch (IpcException)
            {
                // The next read after reconnect uses the same cursor and detects a new worker.
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
