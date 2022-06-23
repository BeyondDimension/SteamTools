using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using System.Application.Models;
using System.Application.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Application.UI.Views.Pages
{
    public partial class ProxyChartView : UserControl
    {
        private CartesianChart chart;

        private readonly LineSeries<RateTick> readSeries = new LineSeries<RateTick>
        {
            Name = "上传",
            Fill = null,
            GeometrySize = 0,
            LineSmoothness = 1,
            TooltipLabelFormatter = (e) => $"{FlowStatistics.ToNetworkSizeString((long)e.PrimaryValue)}/s",
            Mapping = (rate, point) =>
            {
                point.PrimaryValue = rate.Rate;
                point.SecondaryValue = rate.Timestamp;
            }
        };

        private readonly LineSeries<RateTick> writeSeries = new LineSeries<RateTick>
        {
            Name = "下载",
            Fill = null,
            GeometrySize = 0,
            LineSmoothness = 1,
            TooltipLabelFormatter = (e) => $"{FlowStatistics.ToNetworkSizeString((long)e.PrimaryValue)}/s",
            Mapping = (rate, point) =>
            {
                point.PrimaryValue = rate.Rate;
                point.SecondaryValue = rate.Timestamp;
            }
        };

        //public ISeries[] Series { get; set; }

        private List<RateTick> writes = new List<RateTick>();
        private List<RateTick> reads = new List<RateTick>();

        public Func<double, string> XFormatter { get; } = timestamp => ((long)timestamp).ToDateTimeS().ToString("HH:mm:ss");

        public Func<double, string> YFormatter { get; } = value => $"{FlowStatistics.ToNetworkSizeString((long)value)}/s";

        public ProxyChartView()
        {
            InitializeComponent();

            chart = this.FindControl<CartesianChart>("Chart");

            this.readSeries.Values = reads;
            this.writeSeries.Values = writes;

            if (chart != null)
            {
                chart.Series = new ISeries[] { readSeries, writeSeries };
                chart.XAxes = new Axis[] { new Axis { Labeler = XFormatter, UnitWidth = 100 } };
                chart.YAxes = new Axis[] { new Axis { Labeler = YFormatter, MinLimit = 0 } };
            }

            FlushFlowChartAsync();
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

        private async void FlushFlowChartAsync()
        {
            while (true)
            {
                try
                {
                    if (!ProxyService.Current.ProxyStatus) continue;

                    var flowStatistics = await IHttpService.Instance.GetAsync<FlowStatistics>("https://localhost/flowStatistics");
                    if (flowStatistics == null)
                    {
                        continue;
                    }

                    //this.textBlockRead.Text = FlowStatistics.ToNetworkSizeString(flowStatistics.TotalRead);
                    //this.textBlockWrite.Text = FlowStatistics.ToNetworkSizeString(flowStatistics.TotalWrite);

                    var timestamp = GetTimestamp(DateTime.Now);

                    reads.Add(new RateTick(flowStatistics.ReadRate, timestamp));
                    writes.Add(new RateTick(flowStatistics.WriteRate, timestamp));

                    if (this.reads.Count > 60)
                    {
                        this.reads.RemoveAt(0);
                        this.reads.RemoveAt(0);
                    }

                    chart.Series = new ISeries[] { readSeries, writeSeries };
                }
                catch (Exception)
                {
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(1d));
                }
            }
        }
    }
}
