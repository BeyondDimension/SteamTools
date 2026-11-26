using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace BD.WTTS.UI.Views.Controls;

public sealed class StorageBreakdownBar : Control
{
    public static IBrush SavesBrush { get; } = new ImmutableSolidColorBrush(Color.FromRgb(0x1A, 0x9F, 0xFF));
    public static IBrush ScreenshotsBrush { get; } = new ImmutableSolidColorBrush(Color.FromRgb(0x4C, 0xC3, 0x55));
    public static IBrush BackupsBrush { get; } = new ImmutableSolidColorBrush(Color.FromRgb(0xF5, 0xB7, 0x31));

    static readonly IBrush[] segmentBrushes = [SavesBrush, ScreenshotsBrush, BackupsBrush];

    public static readonly StyledProperty<IBrush?> BackgroundProperty = Border.BackgroundProperty.AddOwner<StorageBreakdownBar>();
    public static readonly StyledProperty<long> SavesBytesProperty = AvaloniaProperty.Register<StorageBreakdownBar, long>(nameof(SavesBytes));
    public static readonly StyledProperty<long> ScreenshotsBytesProperty = AvaloniaProperty.Register<StorageBreakdownBar, long>(nameof(ScreenshotsBytes));
    public static readonly StyledProperty<long> BackupsBytesProperty = AvaloniaProperty.Register<StorageBreakdownBar, long>(nameof(BackupsBytes));

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public long SavesBytes
    {
        get => GetValue(SavesBytesProperty);
        set => SetValue(SavesBytesProperty, value);
    }

    public long ScreenshotsBytes
    {
        get => GetValue(ScreenshotsBytesProperty);
        set => SetValue(ScreenshotsBytesProperty, value);
    }

    public long BackupsBytes
    {
        get => GetValue(BackupsBytesProperty);
        set => SetValue(BackupsBytesProperty, value);
    }

    static StorageBreakdownBar() =>
        AffectsRender<StorageBreakdownBar>(BackgroundProperty, SavesBytesProperty, ScreenshotsBytesProperty, BackupsBytesProperty);

    public override void Render(DrawingContext context)
    {
        var rect = new Rect(Bounds.Size);
        if (rect.Width <= 0 || rect.Height <= 0)
            return;

        var radius = Math.Min(rect.Width, rect.Height) / 2;
        using var clip = context.PushClip(new RoundedRect(rect, radius));
        context.DrawRectangle(Background, null, rect);

        Span<long> sizes = [Math.Max(0, SavesBytes), Math.Max(0, ScreenshotsBytes), Math.Max(0, BackupsBytes)];
        var total = (double)sizes[0] + sizes[1] + sizes[2];
        if (total == 0)
            return;

        var count = 0;
        foreach (var size in sizes)
            count += size > 0 ? 1 : 0;

        var minimum = Math.Min(4, rect.Width / count);
        Span<double> widths = stackalloc double[sizes.Length];
        double deficit = 0, reducible = 0;
        for (var i = 0; i < sizes.Length; i++)
        {
            if (sizes[i] == 0)
                continue;

            var width = rect.Width * (sizes[i] / total);
            widths[i] = Math.Max(minimum, width);
            deficit += Math.Max(0, minimum - width);
            reducible += Math.Max(0, width - minimum);
        }

        double x = 0;
        for (var i = 0; i < sizes.Length; i++)
        {
            if (sizes[i] == 0)
                continue;

            var width = widths[i];
            if (reducible > 0 && width > minimum)
                width -= deficit * (width - minimum) / reducible;

            var end = --count == 0 ? rect.Width : Math.Min(x + width, rect.Width);
            context.DrawRectangle(segmentBrushes[i], null, new Rect(x, 0, end - x, rect.Height));
            x = end;
        }
    }
}
