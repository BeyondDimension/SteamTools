using System.Text;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using BD.WTTS.Services;
using IOPath = System.IO.Path;

namespace BD.WTTS.UI.Views.Controls;

public sealed class PathLink : HyperlinkButton
{
    public static readonly StyledProperty<string?> PathProperty =
        AvaloniaProperty.Register<PathLink, string?>(nameof(Path));

    const string EmptyPathText = "—";
    readonly TextBlock label = new() { Text = EmptyPathText, TextWrapping = TextWrapping.Wrap };

    public PathLink()
    {
        Content = label;
        Padding = default;
        MinWidth = 0;
        MinHeight = 0;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        UpdateIsEffectivelyEnabled();
    }

    public string? Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(HyperlinkButton);

    protected override bool IsEnabledCore =>
        base.IsEnabledCore && !string.IsNullOrWhiteSpace(Path) && IOPath.IsPathFullyQualified(Path);

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property != PathProperty)
            return;

        var path = change.GetNewValue<string?>();
        label.Text = string.IsNullOrWhiteSpace(path) ? EmptyPathText : FormatForDisplay(path);
        ToolTip.SetTip(this, path);
        AutomationProperties.SetName(this, path);
        UpdateIsEffectivelyEnabled();
    }

    protected override void OnClick()
    {
        var path = Path;
        var useDefaultAction = Command is null && NavigateUri is null;
        base.OnClick();
        if (!useDefaultAction || Design.IsDesignMode || string.IsNullOrWhiteSpace(path) || !IOPath.IsPathFullyQualified(path))
            return;

        try
        {
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                IPlatformService.Instance.OpenFolderByDirectoryPath(directory);
                return;
            }

            var file = new FileInfo(path);
            if (file.Exists)
                IPlatformService.Instance.OpenFolderSelectFilePath(file);
            else
                Toast.Show(ToastIcon.Warning, Strings.FilePathNotExist);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(nameof(PathLink));
        }
    }

    static string FormatForDisplay(string path)
    {
        var hasDriveRoot = path.Length >= 3 && path[1] == ':' && IsSeparator(path[2]);
        var rootEnd = hasDriveRoot ? 3 : 0;
        while (rootEnd < path.Length && IsSeparator(path[rootEnd]))
            rootEnd++;

        // Break after directory separators, and keep C:\ or leading slashes with the first component.
        var text = new StringBuilder(path.Length);
        for (var i = 0; i < path.Length; i++)
        {
            var ch = path[i];
            text.Append(ch);
            if (i + 1 == path.Length)
                break;
            if (hasDriveRoot && i == 1 || IsSeparator(ch) && i < rootEnd)
                text.Append('\u2060');
            else if (IsSeparator(ch) && !IsSeparator(path[i + 1]))
                text.Append('\u200B');
        }
        return text.ToString();
    }

    static bool IsSeparator(char ch) => ch is '\\' or '/';
}
