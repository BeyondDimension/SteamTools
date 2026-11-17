using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace BD.WTTS.UI.Views.Controls;

/// <summary>
/// A renderer-independent, one-minute byte-rate chart. Mutations belong to the UI thread.
/// History, interpolated vertices and label caches are bounded; only active transitions tick.
/// </summary>
public sealed class TrafficChart : Control
{
    public const int HistoryCapacity = 61;
    const double WindowSeconds = 60;
    const int Subdivisions = 6;
    const double TransitionSeconds = .45;
    public static readonly StyledProperty<bool> IsActiveProperty =
        AvaloniaProperty.Register<TrafficChart, bool>(nameof(IsActive), true);
    public static readonly StyledProperty<string> UploadLabelProperty =
        AvaloniaProperty.Register<TrafficChart, string>(nameof(UploadLabel), string.Empty);
    public static readonly StyledProperty<string> DownloadLabelProperty =
        AvaloniaProperty.Register<TrafficChart, string>(nameof(DownloadLabel), string.Empty);
    public static readonly StyledProperty<string> TotalUploadLabelProperty =
        AvaloniaProperty.Register<TrafficChart, string>(nameof(TotalUploadLabel), string.Empty);
    public static readonly StyledProperty<string> TotalDownloadLabelProperty =
        AvaloniaProperty.Register<TrafficChart, string>(nameof(TotalDownloadLabel), string.Empty);

    static readonly ImmutableSolidColorBrush UploadBrush = new(0xFF18A9DB);
    static readonly ImmutableSolidColorBrush DownloadBrush = new(0xFF9B78ED);
    static readonly ImmutableSolidColorBrush UploadFill = new(0x1618A9DB);
    static readonly ImmutableSolidColorBrush DownloadFill = new(0x169B78ED);
    static readonly ImmutablePen UploadPen = new(UploadBrush, 2.25, lineCap: PenLineCap.Round);
    static readonly ImmutablePen DownloadPen = new(DownloadBrush, 2.25, lineCap: PenLineCap.Round);
    static readonly Typeface Typeface = new(FontFamily.Default);
    readonly TrafficSample[] samples = new TrafficSample[HistoryCapacity];
    readonly Point[] uploadVertices = new Point[(HistoryCapacity - 1) * Subdivisions + 1];
    readonly Point[] downloadVertices = new Point[(HistoryCapacity - 1) * Subdivisions + 1];
    readonly FormattedText?[] rateLabels = new FormattedText?[5];
    readonly FormattedText?[] timeLabels = new FormattedText?[5];
    readonly List<Visual> visibilityAncestors = new();
    readonly DispatcherTimer animationTimer;
    StreamGeometry? uploadArea;
    StreamGeometry? downloadArea;
    ImmutableSolidColorBrush surface = new(0xFFFFFFFF);
    ImmutableSolidColorBrush text = new(0xFF526175);
    ImmutablePen gridPen = new(0x14526175);
    ImmutablePen borderPen = new(0x20526175);
    ImmutablePen crosshairPen = new(0x80526175, 1, lineCap: PenLineCap.Round);
    FormattedText? uploadLegend;
    FormattedText? downloadLegend;
    FormattedText? hoverTime;
    FormattedText? hoverUpload;
    FormattedText? hoverDownload;
    int head;
    int count;
    int vertexCount;
    int hoverIndex = -1;
    long totalUploadBytes;
    long totalDownloadBytes;
    long animationStart;
    double displayedEnd;
    double sourceEnd;
    double targetEnd;
    double displayedScale = 1024;
    double sourceScale = 1024;
    double targetScale = 1024;
    double geometryOrigin;
    double geometryScale = 1024;
    bool attached;
    bool inViewport = true;
    bool presentationActive;
    bool textDirty = true;
    bool legendDirty = true;
    bool geometryDirty = true;
    Point? hoverPosition;
    Rect plot;

    public bool IsActive { get => GetValue(IsActiveProperty); set => SetValue(IsActiveProperty, value); }
    public string UploadLabel { get => GetValue(UploadLabelProperty); set => SetValue(UploadLabelProperty, value); }
    public string DownloadLabel { get => GetValue(DownloadLabelProperty); set => SetValue(DownloadLabelProperty, value); }
    public string TotalUploadLabel { get => GetValue(TotalUploadLabelProperty); set => SetValue(TotalUploadLabelProperty, value); }
    public string TotalDownloadLabel { get => GetValue(TotalDownloadLabelProperty); set => SetValue(TotalDownloadLabelProperty, value); }
    public int SampleCount => count;
    public double ScaleMaximum => displayedScale;
    public bool IsAnimationRunning => animationTimer.IsEnabled;
    public bool IsPresentationActive => presentationActive;
    public Rect PlotBounds => plot;
    public TrafficSample? HoveredSample => hoverIndex < 0 ? null : GetSample(hoverIndex);
    public event EventHandler? PresentationActiveChanged;

    public TrafficChart()
    {
        ClipToBounds = true;
        MinHeight = 150;
        animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        animationTimer.Tick += Animate;
        EffectiveViewportChanged += (_, e) =>
        {
            inViewport = e.EffectiveViewport.Intersects(new Rect(Bounds.Size));
            UpdatePresentation();
        };
        ActualThemeVariantChanged += (_, _) => UpdatePalette();
    }

    public readonly record struct TrafficSample(DateTimeOffset Timestamp, double UploadBytesPerSecond, double DownloadBytesPerSecond);

    /// <summary>Returns the exact stored rates, oldest first (not smoothed or converted to bits).</summary>
    public TrafficSample GetSample(int index)
    {
        if ((uint)index >= (uint)count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return samples[(head + index) % HistoryCapacity];
    }

    /// <summary>
    /// Appends bytes/second. Negative and non-finite readings become zero. A duplicate timestamp
    /// replaces the latest sample; older timestamps are ignored rather than bending time backwards.
    /// </summary>
    public void AppendSample(DateTimeOffset timestamp, double uploadBytesPerSecond, double downloadBytesPerSecond)
    {
        Dispatcher.UIThread.VerifyAccess();
        var seconds = timestamp.ToUnixTimeMilliseconds() / 1000d;
        var sample = new TrafficSample(timestamp, Sanitize(uploadBytesPerSecond), Sanitize(downloadBytesPerSecond));
        if (count > 0)
        {
            var previous = GetSample(count - 1).Timestamp;
            if (timestamp < previous) return;
            if (timestamp == previous)
                samples[(head + count - 1) % HistoryCapacity] = sample;
            else
                Add(sample);
        }
        else
        {
            Add(sample);
            displayedEnd = seconds;
        }
        // Keep one predecessor at the left edge for continuous clipping of the curve.
        while (count > 2 && GetSample(1).Timestamp < timestamp.AddSeconds(-WindowSeconds))
        {
            head = (head + 1) % HistoryCapacity;
            count--;
        }
        double peak = 0;
        for (var i = 0; i < count; i++)
        {
            var item = GetSample(i);
            peak = Math.Max(peak, Math.Max(item.UploadBytesPerSecond, item.DownloadBytesPerSecond));
        }
        var nextScale = NiceScale(peak);
        // Hysteresis prevents near-threshold rates from making the axis breathe every second.
        if (nextScale > targetScale || peak < targetScale * .4)
            targetScale = nextScale;
        sourceScale = displayedScale;
        sourceEnd = displayedEnd;
        targetEnd = seconds;
        geometryDirty = textDirty = true;
        hoverIndex = -1;
        animationStart = Stopwatch.GetTimestamp();
        if (presentationActive && count > 1)
            animationTimer.Start();
        else
            FinishTransition();
        InvalidateVisual();
    }

    void Add(TrafficSample sample)
    {
        if (count == HistoryCapacity)
        {
            head = (head + 1) % HistoryCapacity;
            count--;
        }
        samples[(head + count++) % HistoryCapacity] = sample;
    }

    /// <summary>Updates cumulative byte counts independently of the sampled rates.</summary>
    public void SetTotals(long uploadBytes, long downloadBytes)
    {
        Dispatcher.UIThread.VerifyAccess();
        if (totalUploadBytes == uploadBytes && totalDownloadBytes == downloadBytes) return;
        totalUploadBytes = uploadBytes;
        totalDownloadBytes = downloadBytes;
        legendDirty = true;
        InvalidateVisual();
    }

    public void Clear()
    {
        Dispatcher.UIThread.VerifyAccess();
        animationTimer.Stop();
        head = count = vertexCount = 0;
        totalUploadBytes = totalDownloadBytes = 0;
        legendDirty = true;
        hoverIndex = -1;
        hoverPosition = null;
        displayedEnd = sourceEnd = targetEnd = 0;
        displayedScale = sourceScale = targetScale = 1024;
        uploadArea = downloadArea = null;
        geometryDirty = textDirty = true;
        InvalidateVisual();
    }

    static double Sanitize(double value) => double.IsFinite(value) && value > 0 ? value : 0;

    static double NiceScale(double peak)
    {
        var desired = Math.Max(1024, peak <= double.MaxValue / 1.15 ? peak * 1.15 : peak);
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(desired)));
        var fraction = desired / magnitude;
        var step = fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 2.5 ? 2.5 : fraction <= 5 ? 5 : 10;
        return magnitude <= double.MaxValue / step ? step * magnitude : double.MaxValue;
    }

    void Animate(object? sender, EventArgs e)
    {
        if (!presentationActive)
        {
            FinishTransition();
            return;
        }
        var progress = Math.Min(1, Stopwatch.GetElapsedTime(animationStart).TotalSeconds / TransitionSeconds);
        var eased = 1 - Math.Pow(1 - progress, 3);
        displayedEnd = sourceEnd + (targetEnd - sourceEnd) * eased;
        // Interpolate in log space: a burst spanning units should not make all but the
        // first frame of the animation indistinguishable from its final scale.
        displayedScale = Math.Exp(Math.Log(sourceScale) + (Math.Log(targetScale) - Math.Log(sourceScale)) * eased);
        if (progress >= 1) FinishTransition();
        InvalidateVisual();
    }

    void FinishTransition()
    {
        animationTimer.Stop();
        displayedEnd = targetEnd;
        displayedScale = targetScale;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        attached = true;
        inViewport = true;
        foreach (var ancestor in this.GetVisualAncestors())
        {
            visibilityAncestors.Add(ancestor);
            ancestor.PropertyChanged += AncestorPropertyChanged;
        }
        UpdatePalette();
        UpdatePresentation();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        attached = false;
        foreach (var ancestor in visibilityAncestors)
            ancestor.PropertyChanged -= AncestorPropertyChanged;
        visibilityAncestors.Clear();
        hoverPosition = null;
        hoverIndex = -1;
        UpdatePresentation();
        base.OnDetachedFromVisualTree(e);
    }

    void AncestorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty) UpdatePresentation();
    }

    void UpdatePresentation()
    {
        // Read the ancestors directly: their PropertyChanged notification can precede
        // Avalonia's propagation of IsEffectivelyVisible to descendants.
        var active = attached && IsActive && IsVisible && inViewport;
        for (var i = 0; active && i < visibilityAncestors.Count; i++)
            active = visibilityAncestors[i].IsVisible;
        if (active == presentationActive) return;
        presentationActive = active;
        if (!active)
        {
            FinishTransition();
            hoverPosition = null;
            hoverIndex = -1;
        }
        InvalidateVisual();
        PresentationActiveChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsActiveProperty || change.Property == IsVisibleProperty)
            UpdatePresentation();
        if (change.Property == UploadLabelProperty || change.Property == DownloadLabelProperty ||
            change.Property == TotalUploadLabelProperty || change.Property == TotalDownloadLabelProperty)
        {
            textDirty = true;
            legendDirty = true;
            hoverIndex = -1;
            InvalidateVisual();
        }
        if (change.Property == BoundsProperty)
        {
            geometryDirty = true;
            InvalidateVisual();
        }
    }

    void UpdatePalette()
    {
        var dark = ActualThemeVariant == ThemeVariant.Dark;
        surface = new(dark ? 0xFF202733u : 0xFFFFFFFFu);
        text = new(dark ? 0xFFBAC7DAu : 0xFF526175u);
        gridPen = new(dark ? 0x16C2D3E8u : 0x14526175u);
        borderPen = new(dark ? 0x28C2D3E8u : 0x20526175u);
        crosshairPen = new(dark ? 0x90C2D3E8u : 0x80526175u);
        textDirty = true;
        hoverIndex = -1;
        InvalidateVisual();
    }

    FormattedText Label(string value, IBrush brush, double size = 11) =>
        new(value, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, size, brush);

    void UpdateText()
    {
        if (legendDirty)
        {
            uploadLegend = Label($"{TotalUploadLabel}  {IOPath.GetDisplayFileSizeString(totalUploadBytes)}", UploadBrush, 12);
            downloadLegend = Label($"{TotalDownloadLabel}  {IOPath.GetDisplayFileSizeString(totalDownloadBytes)}", DownloadBrush, 12);
            legendDirty = false;
        }
        if (!textDirty) return;
        for (var i = 0; i < 5; i++)
        {
            rateLabels[i] = Label(FormatRate(targetScale * (i / 4d)), text);
            timeLabels[i] = Label(count == 0 ? "−" : DateTimeOffset.FromUnixTimeMilliseconds((long)((targetEnd - WindowSeconds + i * 15) * 1000)).ToLocalTime().ToString("HH:mm:ss"), text);
        }
        textDirty = false;
    }

    static string FormatRate(double value) => $"{IOPath.GetDisplayFileSizeString(value)}/s";

    void RebuildGeometry()
    {
        plot = new Rect(76, 44, Math.Max(1, Bounds.Width - 92), Math.Max(1, Bounds.Height - 72));
        vertexCount = 0;
        if (count == 0)
        {
            geometryDirty = false;
            return;
        }
        geometryOrigin = targetEnd - WindowSeconds;
        geometryScale = targetScale;
        var previous = GetSample(0);
        uploadVertices[0] = Vertex(previous.Timestamp, previous.UploadBytesPerSecond);
        downloadVertices[0] = Vertex(previous.Timestamp, previous.DownloadBytesPerSecond);
        vertexCount = 1;
        for (var i = 1; i < count; i++)
        {
            var current = GetSample(i);
            var start = (previous.Timestamp.ToUnixTimeMilliseconds() / 1000d - geometryOrigin) / WindowSeconds;
            var end = (current.Timestamp.ToUnixTimeMilliseconds() / 1000d - geometryOrigin) / WindowSeconds;
            for (var segment = 1; segment <= Subdivisions; segment++)
            {
                var t = segment / (double)Subdivisions;
                // Smoothstep is monotone between exact endpoints: zero cannot become negative,
                // and bursts do not acquire invented overshoots from a spline tangent.
                var smooth = t * t * (3 - 2 * t);
                var x = (start + (end - start) * t) * plot.Width;
                uploadVertices[vertexCount] = new Point(x, -(previous.UploadBytesPerSecond / geometryScale + (current.UploadBytesPerSecond / geometryScale - previous.UploadBytesPerSecond / geometryScale) * smooth) * plot.Height);
                downloadVertices[vertexCount] = new Point(x, -(previous.DownloadBytesPerSecond / geometryScale + (current.DownloadBytesPerSecond / geometryScale - previous.DownloadBytesPerSecond / geometryScale) * smooth) * plot.Height);
                vertexCount++;
            }
            previous = current;
        }
        uploadArea = BuildArea(uploadVertices);
        downloadArea = BuildArea(downloadVertices);
        geometryDirty = false;
    }

    Point Vertex(DateTimeOffset timestamp, double value) => new(
        (timestamp.ToUnixTimeMilliseconds() / 1000d - geometryOrigin) / WindowSeconds * plot.Width,
        -(value / geometryScale) * plot.Height);

    StreamGeometry BuildArea(Point[] vertices)
    {
        var geometry = new StreamGeometry();
        using var writer = geometry.Open();
        writer.BeginFigure(new Point(vertices[0].X, 0), true);
        for (var i = 0; i < vertexCount; i++) writer.LineTo(vertices[i]);
        writer.LineTo(new Point(vertices[vertexCount - 1].X, 0));
        writer.EndFigure(true);
        return geometry;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Bounds.Width < 120 || Bounds.Height < 80) return;
        if (textDirty || legendDirty) UpdateText();
        if (geometryDirty) RebuildGeometry();
        var legendX = Math.Max(16, Bounds.Width / 2);
        using (context.PushClip(new Rect(12, 4, Math.Max(1, legendX - 18), 32)))
        {
            context.DrawEllipse(UploadBrush, null, new Point(20, 22), 3.5, 3.5);
            context.DrawText(uploadLegend!, new Point(31, 14));
        }
        using (context.PushClip(new Rect(legendX, 4, Math.Max(1, Bounds.Width - legendX - 12), 32)))
        {
            context.DrawEllipse(DownloadBrush, null, new Point(legendX + 4, 22), 3.5, 3.5);
            context.DrawText(downloadLegend!, new Point(legendX + 15, 14));
        }
        for (var i = 0; i < 5; i++)
        {
            var label = rateLabels[i]!;
            // Target labels slide with their value while the scale settles; no new text is
            // shaped on an animation frame. The baseline is always the exact zero rate.
            var labelY = i == 0 ? plot.Bottom : plot.Bottom - plot.Height * (targetScale / displayedScale) * (i / 4d);
            if (labelY >= plot.Top - 1 && labelY <= plot.Bottom + 1)
            {
                context.DrawLine(gridPen, new Point(plot.Left, labelY), new Point(plot.Right, labelY));
                context.DrawText(label, new Point(Math.Max(4, plot.Left - label.Width - 10), labelY - label.Height / 2));
            }
            if (plot.Width < 300 && i % 2 == 1) continue;
            var time = timeLabels[i]!;
            var x = plot.Left + plot.Width * i / 4 + (targetEnd - displayedEnd) / WindowSeconds * plot.Width;
            if (x > plot.Right + 1) continue;
            context.DrawLine(gridPen, new Point(x, plot.Top), new Point(x, plot.Bottom));
            context.DrawText(time, new Point(Math.Clamp(x - time.Width / 2, plot.Left, Math.Max(plot.Left, plot.Right - time.Width)), plot.Bottom + 10));
        }
        if (vertexCount > 0)
        {
            var dx = plot.Left + (targetEnd - displayedEnd) / WindowSeconds * plot.Width;
            // Offscreen peaks need not produce unbounded native coordinates during an
            // extreme scale transition. Clipping still gives the same visible chart.
            var sy = Math.Min(1_000_000, geometryScale / displayedScale);
            using (context.PushClip(plot.Inflate(new Thickness(0, 2))))
            {
                // The fills are cached geometry. Lines transform cached vertices explicitly,
                // keeping their stroke width constant even when a burst changes Y by 1000x.
                using (context.PushTransform(new Matrix(1, 0, 0, sy, dx, plot.Bottom)))
                {
                    context.DrawGeometry(UploadFill, null, uploadArea!);
                    context.DrawGeometry(DownloadFill, null, downloadArea!);
                }
                DrawSeries(context, uploadVertices, UploadPen, dx, sy);
                DrawSeries(context, downloadVertices, DownloadPen, dx, sy);
            }
        }
        UpdateHover();
        if (hoverIndex >= 0) DrawHover(context);
    }

    void DrawSeries(DrawingContext context, Point[] vertices, IPen pen, double dx, double sy)
    {
        var previous = new Point(vertices[0].X + dx, plot.Bottom + vertices[0].Y * sy);
        if (vertexCount == 1) context.DrawEllipse(pen.Brush, null, previous, 3, 3);
        for (var i = 1; i < vertexCount; i++)
        {
            var current = new Point(vertices[i].X + dx, plot.Bottom + vertices[i].Y * sy);
            if (current.X >= plot.Left && previous.X <= plot.Right)
                context.DrawLine(pen, previous, current);
            previous = current;
        }
    }

    /// <summary>Selects the nearest visible sample in control coordinates; null clears hover.</summary>
    public void SetHoverPosition(Point? position)
    {
        Dispatcher.UIThread.VerifyAccess();
        hoverPosition = position;
        UpdateHover();
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        SetHoverPosition(e.GetPosition(this));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetHoverPosition(null);
    }

    void UpdateHover()
    {
        var nearest = -1;
        var distance = double.PositiveInfinity;
        if (hoverPosition is { } pointer && plot.Contains(pointer))
        {
            for (var i = 0; i < count; i++)
            {
                var sample = GetSample(i);
                var x = SampleX(sample);
                if (x < plot.Left || x > plot.Right) continue;
                var delta = Math.Abs(x - pointer.X);
                if (delta < distance) { distance = delta; nearest = i; }
            }
        }
        if (nearest == hoverIndex) return;
        hoverIndex = nearest;
        if (nearest < 0) return;
        var item = GetSample(nearest);
        hoverTime = Label(item.Timestamp.ToLocalTime().ToString("HH:mm:ss"), text);
        hoverUpload = Label($"{UploadLabel}  {FormatRate(item.UploadBytesPerSecond)}", UploadBrush, 12);
        hoverDownload = Label($"{DownloadLabel}  {FormatRate(item.DownloadBytesPerSecond)}", DownloadBrush, 12);
    }

    double SampleX(TrafficSample sample) => plot.Right - (displayedEnd - sample.Timestamp.ToUnixTimeMilliseconds() / 1000d) / WindowSeconds * plot.Width;

    void DrawHover(DrawingContext context)
    {
        var sample = GetSample(hoverIndex);
        var x = SampleX(sample);
        context.DrawLine(crosshairPen, new Point(x, plot.Top), new Point(x, plot.Bottom));
        using (context.PushClip(plot.Inflate(new Thickness(0, 5))))
        {
            context.DrawEllipse(surface, UploadPen, new Point(x, plot.Bottom - Math.Min(1_000_000, sample.UploadBytesPerSecond / displayedScale) * plot.Height), 4, 4);
            context.DrawEllipse(surface, DownloadPen, new Point(x, plot.Bottom - Math.Min(1_000_000, sample.DownloadBytesPerSecond / displayedScale) * plot.Height), 4, 4);
        }
        var width = Math.Min(plot.Width, Math.Max(hoverUpload!.Width, hoverDownload!.Width) + 24);
        var left = Math.Clamp(x + 12, plot.Left, Math.Max(plot.Left, plot.Right - width));
        var box = new Rect(left, plot.Top + 6, width, 80);
        context.DrawRectangle(surface, borderPen, box, 8, 8);
        using (context.PushClip(box.Deflate(8)))
        {
            context.DrawText(hoverTime!, new Point(left + 12, box.Top + 9));
            context.DrawText(hoverUpload, new Point(left + 12, box.Top + 30));
            context.DrawText(hoverDownload!, new Point(left + 12, box.Top + 51));
        }
    }
}
