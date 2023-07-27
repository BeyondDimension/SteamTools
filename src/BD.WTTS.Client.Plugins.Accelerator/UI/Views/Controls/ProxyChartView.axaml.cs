using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LiveChartsCore;
using LiveChartsCore.Motion;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;

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
        YToolTipLabelFormatter = (e) => $"Upload {IOPath.GetDisplayFileSizeString(e.Coordinate.PrimaryValue)}/s",
        Mapping = (rate, point) =>
        {
            point.Coordinate = new(rate.Timestamp, rate.Rate);
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
        //Stroke = LiveChartsSkiaSharp.DefaultPaint,
        YToolTipLabelFormatter = (e) => $"Download {IOPath.GetDisplayFileSizeString(e.Coordinate.PrimaryValue)}/s",
        Mapping = (rate, point) =>
        {
            point.Coordinate = new(rate.Timestamp, rate.Rate);
            //point.PrimaryValue = rate.Rate;
            //point.SecondaryValue = rate.Timestamp;
        }
    };

    //public ISeries[] Series { get; set; }

    private readonly List<RateTick> writes = new();
    private readonly List<RateTick> reads = new();

    public Func<double, string> XFormatter { get; } = timestamp => ((long)timestamp).ToDateTimeS().ToString("HH:mm:ss");

    public Func<double, string> YFormatter { get; } = value => $"{IOPath.GetDisplayFileSizeString(value)}/s";

    public ProxyChartView()
    {
        InitializeComponent();

        this.readSeries.Values = reads;
        this.writeSeries.Values = writes;

        if (Chart != null)
        {
            Chart.UpdateFinished += Chart_UpdateFinished;
            Chart.Series = new ISeries[] { readSeries, writeSeries };
            Chart.XAxes = new Axis[] { new Axis { Labeler = XFormatter } };
            Chart.YAxes = new Axis[] { new Axis { Labeler = YFormatter, MinLimit = 0 } };
        }

        CancellationTokenSource? cancellation = null;

        ProxyService.Current.WhenAnyValue(x => x.ProxyStatus)
            .Subscribe(x =>
            {
                if (x)
                {
                    cancellation = new CancellationTokenSource();
                    FlushFlowChartAsync(cancellation.Token);
                }
                else
                {
                    if (cancellation != null)
                    {
                        cancellation.Cancel();
                        cancellation.Dispose();
                    }
                    reads.Clear();
                    writes.Clear();
                }
            });
    }

    private static double GetTimestamp(DateTime dateTime) => dateTime.ToUnixTimeSeconds();

    private class RateTick
    {
        public double Rate { get; }

        public double Timestamp { get; }

        public RateTick(double rate, double timestamp)
        {
            this.Rate = rate;
            this.Timestamp = timestamp;
        }
    }

    private async void FlushFlowChartAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var flowStatistics = IReverseProxyService.Constants.Instance.GetFlowStatistics();
                if (flowStatistics == null)
                    continue;

                var isAttachedToVisualTree = this.IsAttachedToVisualTree();

                if (isAttachedToVisualTree)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        textBlockRead.Text = IOPath.GetDisplayFileSizeString(flowStatistics.TotalRead);
                        textBlockWrite.Text = IOPath.GetDisplayFileSizeString(flowStatistics.TotalWrite);
                    });
                }

                var timestamp = GetTimestamp(DateTime.Now);

                reads.Add(new RateTick(flowStatistics.ReadRate, timestamp));
                writes.Add(new RateTick(flowStatistics.WriteRate, timestamp));

                if (this.reads.Count > 60)
                {
                    this.reads.RemoveAt(0);
                    this.writes.RemoveAt(0);
                }

                if (Chart != null && isAttachedToVisualTree)
                    Dispatcher.UIThread.Post(() =>
                    {
                        Chart.Series = new ISeries[] { readSeries, writeSeries };
                    });
            }
            catch
            {
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(1d), CancellationToken.None);
            }
        }
    }

    //解决内存泄露问题
    //https://github.com/beto-rodriguez/LiveCharts2/issues/1080#issuecomment-1601536016
    private static void Chart_UpdateFinished(LiveChartsCore.Kernel.Sketches.IChartView<SkiaSharpDrawingContext> chart)
    {
        // Chart library leaks PaintTasks
        if (chart.CoreCanvas != null)
        {
            // Periodically clean up drawables, this may cause the chart to blip if the user mouses over it during this time
            if (chart.CoreCanvas.DrawablesCount > 50)
            {
                chart.CoreCanvas.SetPaintTasks(new HashSet<LiveChartsCore.Drawing.IPaint<SkiaSharpDrawingContext>>());
            }
        }
    }
}
