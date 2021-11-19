using System;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;

namespace System.Application.UI.Views.Controls
{
    public class Arc : Control
    {
        static Arc()
        {
            AffectsRender<Arc>(ArcBrushProperty,
                              StrokeProperty,
                              StartAngleProperty,
                              SweepAngleProperty);
        }

        public IBrush ArcBrush
        {
            get => GetValue(ArcBrushProperty);
            set => SetValue(ArcBrushProperty, value);
        }

        public readonly static StyledProperty<IBrush> ArcBrushProperty =
            AvaloniaProperty.Register<Arc, IBrush>(nameof(ArcBrush), new SolidColorBrush(Avalonia.Media.Colors.White), inherits: true, defaultBindingMode: BindingMode.TwoWay);

        public double Stroke
        {
            get => GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public readonly static StyledProperty<double> StrokeProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(Stroke), 10, inherits: true, defaultBindingMode: BindingMode.TwoWay);

        public double StartAngle
        {
            get => GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public readonly static StyledProperty<double> StartAngleProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(StartAngle), 0, inherits: true, defaultBindingMode: BindingMode.TwoWay);

        public double SweepAngle
        {
            get => GetValue(SweepAngleProperty);
            set => SetValue(SweepAngleProperty, value);
        }

        public static readonly StyledProperty<double> SweepAngleProperty =
            AvaloniaProperty.Register<Arc, double>(nameof(SweepAngle), 90, inherits: true, defaultBindingMode: BindingMode.TwoWay);

        public override void Render(DrawingContext context)
        {
            var offsetStroke = 0.5;
            var o = Stroke + offsetStroke;
            
            // Create main circle for draw circle
            var mainCircle =
                new EllipseGeometry(new Rect(o / 2, o / 2, Bounds.Width - o, Bounds.Height - o));

            var paint = new Pen(ArcBrush, Stroke);
            
            // Push generated clip geometry for clipping circle figure
            context.PlatformImpl.PushGeometryClip(GetClip().PlatformImpl);
            context.PlatformImpl.DrawGeometry(SolidColorBrush.Parse("Transparent"), paint, mainCircle.PlatformImpl);
            context.PlatformImpl.PopGeometryClip();
            // Pop clip geometry
            
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }

        // Clip geometry generator
        private StreamGeometry GetClip()
        {
            var offset = StartAngle - 90;
            
            var w = Bounds.Width;
            var h = Bounds.Height;
            
            var halfW = w / 2;
            var halfH = h / 2;

            var sweep = (offset + SweepAngle) / 360;
            
            var path = new StringBuilder($"M {halfW.ToString(CultureInfo.InvariantCulture)} {halfH.ToString(CultureInfo.InvariantCulture)}");
            
            int length = 24;
            
            for (int i = 0; i < length; i++)
            {
                var limit = offset / 360 + i / (double)length;
                
                if (limit > sweep)
                    break;
                
                var r2 = limit * (Math.PI * 2);
                var x2 = halfW + Math.Round(halfW * Math.Cos(r2), 4);
                var y2 = halfH + Math.Round(halfH * Math.Sin(r2), 4);
                
                path.Append($" {x2.ToString(CultureInfo.InvariantCulture)} {y2.ToString(CultureInfo.InvariantCulture)}");
            }
            
            var r3 = sweep * (Math.PI * 2);
            var x3 = halfW + Math.Round(halfW * Math.Cos(r3), 4);
            var y3 = halfH + Math.Round(halfH * Math.Sin(r3), 4);
            
            path.Append($" {x3.ToString(CultureInfo.InvariantCulture)} {y3.ToString(CultureInfo.InvariantCulture)}");

            path.Append(" Z");
            var result = path.ToString().Replace(',', '.');
            return StreamGeometry.Parse(result);
        }
    }
}