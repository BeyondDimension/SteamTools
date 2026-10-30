using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace BD.WTTS.UI.Views.Controls;

public partial class ProxyChartView : UserControl
{
    private readonly LineSeries<RateTick> readSeries = new()
    {
        GeometrySize = 0,
        GeometryFill = null,
        GeometryStroke = null,
        LineSmoothness = 1,
        EnableNullSplitting = false,
        DataLabelsSize = 12,
        //Stroke = LiveChartsSkiaSharp.DefaultPaint,
        Name = Strings.Upload,
        YToolTipLabelFormatter = (e) => $"{IOPath.GetDisplayFileSizeString(e.Coordinate.PrimaryValue)}/s",
        Mapping = (rate, point) =>
        {
            return new(rate.Timestamp, rate.Rate);
            //point.PrimaryValue = rate.Rate;
            //point.SecondaryValue = rate.Timestamp;
        }
    };

    private readonly LineSeries<RateTick> writeSeries = new()
    {
        GeometrySize = 0,
        GeometryFill = null,
        GeometryStroke = null,
        LineSmoothness = 1,
        EnableNullSplitting = false,
        DataLabelsSize = 12,
        Name = Strings.Download,
        //Stroke = LiveChartsSkiaSharp.DefaultPaint,
        YToolTipLabelFormatter = (e) => $"{IOPath.GetDisplayFileSizeString(e.Coordinate.PrimaryValue)}/s",
        Mapping = (rate, point) =>
        {
            return new(rate.Timestamp, rate.Rate);
            //point.PrimaryValue = rate.Rate;
            //point.SecondaryValue = rate.Timestamp;
        }
    };

    private readonly ObservableCollection<RateTick> writes = new();
    private readonly ObservableCollection<RateTick> reads = new();

    public Func<double, string> XFormatter { get; } = timestamp => ((long)timestamp).ToDateTimeS().ToString("HH:mm:ss");

    public Func<double, string> YFormatter { get; } = value => $"{IOPath.GetDisplayFileSizeString(value)}/s";

    CancellationTokenSource? cancellation;
    Task? refreshTask;
    IDisposable? statusSubscription;

    public static readonly AvaloniaProperty<IEnumerable<ISeries>> SeriesProperty =
        AvaloniaProperty.Register<CartesianChart, IEnumerable<ISeries>>("Series", Enumerable.Empty<ISeries>(), inherits: true);

    public ObservableCollection<ISeries> Series
    {
        get
        {
            return (ObservableCollection<ISeries>)GetValue(SeriesProperty);
        }

        set
        {
            SetValue(SeriesProperty, value);
        }
    }

    public ProxyChartView()
    {
        InitializeComponent();

        this.readSeries.Values = reads;
        this.writeSeries.Values = writes;
        Series = [readSeries, writeSeries];

        if (Chart != null)
        {
            //Chart.UpdateFinished += Chart_UpdateFinished;
            //Chart.Series = new ISeries[] { readSeries, writeSeries };
            Chart[!CartesianChart.SeriesProperty] = this[!SeriesProperty];
            Chart.XAxes = new Axis[] { new Axis { Labeler = XFormatter } };
            Chart.YAxes = new Axis[] { new Axis { Labeler = YFormatter, MinLimit = 0 } };
        }

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
            reads.Clear();
            writes.Clear();
        }
        else if (cancellation == null)
        {
            var source = cancellation = new CancellationTokenSource();
            var token = source.Token;
            refreshTask = Task.Run(() => FlushFlowChartAsync(token));
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

    private struct RateTick
    {
        public double Rate { get; }

        public double Timestamp { get; }

        public RateTick(double rate, double timestamp)
        {
            this.Rate = rate;
            this.Timestamp = timestamp;
        }
    }

    async Task FlushFlowChartAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var flowStatistics = IReverseProxyService.Constants.Instance.GetFlowStatistics();
                if (flowStatistics != null)
                {
                    var timestamp = DateTime.Now.ToUnixTimeSeconds();
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (token.IsCancellationRequested)
                            return;

                        textBlockRead.Text = IOPath.GetDisplayFileSizeString(flowStatistics.TotalRead);
                        textBlockWrite.Text = IOPath.GetDisplayFileSizeString(flowStatistics.TotalWrite);
                        reads.Add(new RateTick(flowStatistics.ReadRate, timestamp));
                        writes.Add(new RateTick(flowStatistics.WriteRate, timestamp));

                        if (reads.Count > 60)
                        {
                            reads.RemoveAt(0);
                            writes.RemoveAt(0);
                        }
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

    ////解决内存泄露问题
    ////https://github.com/beto-rodriguez/LiveCharts2/issues/1080#issuecomment-1601536016
    //private static void Chart_UpdateFinished(LiveChartsCore.Kernel.Sketches.IChartView<SkiaSharpDrawingContext> chart)
    //{
    //    // Chart library leaks PaintTasks
    //    if (chart.CoreCanvas != null)
    //    {
    //        // Periodically clean up drawables, this may cause the chart to blip if the user mouses over it during this time
    //        if (chart.CoreCanvas.DrawablesCount > 50)
    //        {
    //            chart.CoreCanvas.SetPaintTasks([]);
    //        }
    //    }
    //}
}
