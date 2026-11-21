using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace BD.WTTS.UI.Views.Controls;

public partial class ProxyChartView : UserControl
{
    CancellationTokenSource? cancellation;
    Task? refreshTask;
    IDisposable? statusSubscription;
    bool attached;
    bool running;

    public ProxyChartView()
    {
        InitializeComponent();
        Chart.IsActive = false;
        Chart.PresentationActiveChanged += (_, _) => UpdateRefreshState();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        attached = true;
        statusSubscription = ProxyService.Current.ObserveValue(nameof(ProxyService.ProxyStatus), static x => x.ProxyStatus)
            .ObserveOn(RxSchedulers.MainThreadScheduler)
            .Subscribe(UpdateProxyStatus);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        attached = false;
        statusSubscription?.Dispose();
        statusSubscription = null;
        Chart.IsActive = false;
        UpdateRefreshState();
        base.OnDetachedFromVisualTree(e);
    }

    void UpdateProxyStatus(bool value)
    {
        running = value;
        Chart.IsActive = value;
        if (!value)
        {
            Chart.Clear();
        }
        UpdateRefreshState();
    }

    void UpdateRefreshState()
    {
        if (!attached || !running || !Chart.IsPresentationActive)
        {
            cancellation?.Cancel();
            return;
        }
        // Retain the old task until its final IPC read has completed. Rapid tab changes
        // and stop/start transitions must never produce overlapping refresh loops.
        if (refreshTask != null) return;
        var source = cancellation = new CancellationTokenSource();
        var task = refreshTask = Task.Run(() => FlushFlowChartAsync(source.Token));
        _ = CompleteRefreshAsync(task, source);
    }

    async Task CompleteRefreshAsync(Task task, CancellationTokenSource source)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException) when (source.IsCancellationRequested)
        {
        }
        finally
        {
            source.Dispose();
            cancellation = null;
            refreshTask = null;
            UpdateRefreshState();
        }
    }

    async Task FlushFlowChartAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var flow = IReverseProxyService.Constants.Instance.GetFlowStatistics();
                if (flow != null)
                {
                    var timestamp = DateTimeOffset.Now;
                    // Await the UI hand-off rather than filling its queue if rendering is busy.
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (token.IsCancellationRequested) return;
                        Chart.SetTotals(flow.TotalRead, flow.TotalWrite);
                        // Proxy read = upload; proxy write = download. Values are already B/s.
                        Chart.AppendSample(timestamp, flow.ReadRate, flow.WriteRate);
                    });
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
            catch
            {
                // The remote service can be unavailable while starting or stopping.
                // Keep the last real sample rather than inventing a zero-rate reading.
            }
            await Task.Delay(1000, token).ConfigureAwait(false);
        }
    }
}
