using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DynamicData.Binding;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.UI.Views.Pages
{
    public partial class ProxyChartView : UserControl
    {
        private readonly CartesianChart? chart;
        private readonly TextBlock textBlockRead;
        private readonly TextBlock textBlockWrite;

        private readonly LineSeries<RateTick> readSeries = new()
        {
            GeometrySize = 0,
            GeometryFill = null,
            GeometryStroke = null,
            LineSmoothness = 1,
            EnableNullSplitting = false,
            DataLabelsSize = 12,
            Stroke = LiveChartsSkiaSharp.DefaultPaint,
            TooltipLabelFormatter = (e) => $"Upload {IOPath.GetDisplayFileSizeString(e.PrimaryValue)}/s",
            Mapping = (rate, point) =>
            {
                point.PrimaryValue = rate.Rate;
                point.SecondaryValue = rate.Timestamp;
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
            Stroke = LiveChartsSkiaSharp.DefaultPaint,
            TooltipLabelFormatter = (e) => $"Download {IOPath.GetDisplayFileSizeString(e.PrimaryValue)}/s",
            Mapping = (rate, point) =>
            {
                point.PrimaryValue = rate.Rate;
                point.SecondaryValue = rate.Timestamp;
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

            chart = this.FindControl<CartesianChart>("Chart");
            textBlockRead = this.FindControl<TextBlock>("textBlockRead");
            textBlockWrite = this.FindControl<TextBlock>("textBlockWrite");

            this.readSeries.Values = reads;
            this.writeSeries.Values = writes;

            if (chart != null)
            {
                chart.Series = new ISeries[] { readSeries, writeSeries };
                chart.XAxes = new Axis[] { new Axis { Labeler = XFormatter } };
                chart.YAxes = new Axis[] { new Axis { Labeler = YFormatter, MinLimit = 0 } };
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
                        cancellation?.Cancel();
                        reads.Clear();
                        writes.Clear();
                    }
                });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
                    var flowStatistics = IReverseProxyService.Instance.GetFlowStatistics();
                    if (flowStatistics == null)
                    {
                        continue;
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        this.textBlockRead.Text = IOPath.GetDisplayFileSizeString(flowStatistics.TotalRead);
                        this.textBlockWrite.Text = IOPath.GetDisplayFileSizeString(flowStatistics.TotalWrite);
                    });

                    var timestamp = GetTimestamp(DateTime.Now);

                    reads.Add(new RateTick(flowStatistics.ReadRate, timestamp));
                    writes.Add(new RateTick(flowStatistics.WriteRate, timestamp));

                    if (this.reads.Count > 60)
                    {
                        this.reads.RemoveAt(0);
                        this.writes.RemoveAt(0);
                    }

                    if (chart != null)
                        Dispatcher.UIThread.Post(() =>
                        {
                            chart.Series = new ISeries[] { readSeries, writeSeries };
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
    }
}
