using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
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

    CancellationTokenSource cancellation = new();

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

        ProxyService.Current.WhenAnyValue(x => x.ProxyStatus)
            .Subscribe(x =>
            {
                if (x)
                {
                    cancellation = new CancellationTokenSource();
                    Task2.InBackground(FlushFlowChartAsync, true);
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

    private void FlushFlowChartAsync()
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                var flowStatistics = IReverseProxyService.Constants.Instance.GetFlowStatistics();
                if (flowStatistics == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                var isAttachedToVisualTree = this.IsAttachedToVisualTree();

                if (isAttachedToVisualTree)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        textBlockRead.Text = IOPath.GetDisplayFileSizeString(flowStatistics.TotalRead);
                        textBlockWrite.Text = IOPath.GetDisplayFileSizeString(flowStatistics.TotalWrite);
                    });
                }

                var timestamp = DateTime.Now.ToUnixTimeSeconds();

                reads.Add(new RateTick(flowStatistics.ReadRate, timestamp));
                writes.Add(new RateTick(flowStatistics.WriteRate, timestamp));

                if (this.reads.Count > 60)
                {
                    this.reads.RemoveAt(0);
                    this.writes.RemoveAt(0);
                }

                //if (Chart != null && isAttachedToVisualTree)
                //{
                //    Dispatcher.UIThread.Post(() =>
                //    {
                //        Chart.Series = new ISeries[] { readSeries, writeSeries };
                //    });
                //}
            }
            catch
            {
            }
            finally
            {
                Thread.Sleep(1000);
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
