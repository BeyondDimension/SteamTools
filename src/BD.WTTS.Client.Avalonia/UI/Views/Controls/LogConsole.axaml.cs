using Avalonia.Automation;
using Avalonia.Input.Platform;
using AvaloniaEdit.Search;
using BD.WTTS.Logging;
using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.Views.Controls;

public partial class LogConsole : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<LogConsole, string?>(nameof(Title));

    public static readonly StyledProperty<LogTextBuffer?> BufferProperty =
        AvaloniaProperty.Register<LogConsole, LogTextBuffer?>(nameof(Buffer));

    public static readonly StyledProperty<int> MaxLinesProperty =
        AvaloniaProperty.Register<LogConsole, int>(nameof(MaxLines), 2000, coerce: static (_, value) => Math.Clamp(value, 1, 10000));

    public static readonly StyledProperty<Control?> CommandContentProperty =
        AvaloniaProperty.Register<LogConsole, Control?>(nameof(CommandContent));

    public string? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

    public LogTextBuffer? Buffer { get => GetValue(BufferProperty); set => SetValue(BufferProperty, value); }

    public int MaxLines { get => GetValue(MaxLinesProperty); set => SetValue(MaxLinesProperty, value); }

    public Control? CommandContent { get => GetValue(CommandContentProperty); set => SetValue(CommandContentProperty, value); }

    readonly DispatcherTimer refresh = new(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(250) };
    SearchPanel? search;
    long bufferOffset;
    VisualVisibilitySubscription? visibility;
    bool scrollPending;

    public LogConsole()
    {
        InitializeComponent();
        LogSyntaxHighlighting.Apply(Editor);
        Editor.Options.AllowScrollBelowDocument = false;
        Editor.Document.UndoStack.SizeLimit = 0;
        refresh.Tick += (_, _) => ReadBuffer();
        Editor.AddHandler(KeyDownEvent, Editor_OnKeyDown, RoutingStrategies.Tunnel);
        UpdateStatus();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        visibility = new(this, UpdateReader);
        UpdateReader();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        visibility?.Dispose();
        visibility = null;
        refresh.Stop();
        CloseSearch();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BufferProperty)
            ResetLog();
        else if (change.Property == MaxLinesProperty && Editor != null)
        {
            if (Buffer != null && change.GetNewValue<int>() > change.GetOldValue<int>())
                ReloadDocument();
            else
                ApplyEdit(0, string.Empty);
        }
    }

    /// <summary>Write a batch from a log source that manages its own updates instead of using <see cref="Buffer"/>.</summary>
    public void WriteText(string text, bool replace = false) => ApplyEdit(replace ? Editor.Document.TextLength : 0, text);

    /// <summary>Reset selection and follow mode when the host changes the log source.</summary>
    public void ResetLog()
    {
        // Buffer can be set by a style before initialization has built the visual tree.
        if (Editor == null) return;
        CloseSearch();
        FollowLog.IsChecked = true;
        ReloadDocument();
    }

    void ReloadDocument()
    {
        bufferOffset = 0;
        ApplyEdit(Editor.Document.TextLength, string.Empty);
        UpdateReader();
    }

    void UpdateReader()
    {
        if (visibility == null || !IsEffectivelyVisible)
        {
            refresh.Stop();
            return;
        }
        if (Buffer == null)
        {
            refresh.Stop();
        }
        else
        {
            ReadBuffer();
            refresh.Start();
        }
        ScrollIfFollowing();
    }

    void ReadBuffer()
    {
        if (Buffer is not { } buffer) return;
        var update = buffer.ReadSince(bufferOffset);
        if (update.End == bufferOffset) return;
        var length = Editor.Document.TextLength;
        var drop = (int)Math.Clamp(update.First - (bufferOffset - length), 0, length);
        bufferOffset = update.End;
        ApplyEdit(drop, update.Text);
    }

    void ApplyEdit(int drop, string text)
    {
        var document = Editor.Document;
        var length = document.TextLength;
        var maxLines = MaxLines;
        var cap = LogTextBuffer.MaxCharacters;
        var start = Math.Max(0, text.Length - cap);
        if (start > 0 && (char.IsLowSurrogate(text[start]) || (text[start] == '\n' && text[start - 1] == '\r')))
            start++;

        // Find the retained suffix before allocating document lines for a large incoming batch.
        var trailingBreak = text.Length != 0 && text[^1] is '\r' or '\n';
        var allowedBreaks = maxLines - (trailingBreak ? 0 : 1);
        var breaks = 0;
        for (var i = text.Length; i > start;)
        {
            var separator = text.AsSpan(start, i - start).LastIndexOfAny('\r', '\n');
            if (separator < 0) break;
            var offset = start + separator;
            if (breaks == allowedBreaks)
            {
                start = offset + 1;
                break;
            }
            breaks++;
            i = offset > start && text[offset] == '\n' && text[offset - 1] == '\r' ? offset - 1 : offset;
        }

        if (start > 0)
            drop = length;
        else if (drop < length)
        {
            var joinsCrLf = text.Length > 0 && text[0] == '\n' && document.GetCharAt(length - 1) == '\r';
            var emptyLastLine = text.Length == 0 ? document.GetLineByNumber(document.LineCount).Length == 0 : trailingBreak;
            var excessLines = document.LineCount + breaks - (joinsCrLf ? 1 : 0) - (emptyLastLine ? 1 : 0) - maxLines;
            if (excessLines > 0)
                drop = Math.Max(drop, document.GetLineByNumber(excessLines + 1).Offset);

            var bySize = length + text.Length - cap;
            if (bySize > drop)
            {
                drop = bySize;
                var line = document.GetLineByOffset(drop);
                if (drop != line.Offset && line.NextLine is { } next && next.Offset < length)
                    drop = next.Offset;
            }

            if (drop > 0 && drop < length)
            {
                var first = document.GetCharAt(drop);
                if (char.IsLowSurrogate(first) || (first == '\n' && document.GetCharAt(drop - 1) == '\r'))
                    drop++;
            }
        }

        if (drop == 0 && start == text.Length) return;
        using (document.RunUpdate())
        {
            if (drop > 0) document.Remove(0, drop);
            if (start < text.Length)
                document.Insert(document.TextLength, start == 0 ? text : text[start..]);
        }
        UpdateStatus();
        ScrollIfFollowing();
    }

    void UpdateStatus()
    {
        var document = Editor.Document;
        var hasText = document.TextLength != 0;
        var lines = document.LineCount - (document.GetLineByNumber(document.LineCount).Length == 0 ? 1 : 0);
        EmptyLog.IsVisible = !hasText;
        CopyLog.IsEnabled = hasText;
        FindLog.IsEnabled = hasText || search is { IsOpened: true };
        LogStatus.Text = string.Format(CultureInfo.CurrentCulture, AppResources.LogConsole_LineCount, lines);
    }

    void ScrollIfFollowing()
    {
        if (visibility == null || !IsEffectivelyVisible || FollowLog.IsChecked != true || scrollPending) return;
        scrollPending = true;
        Dispatcher.UIThread.Post(() =>
        {
            scrollPending = false;
            if (visibility != null && IsEffectivelyVisible && FollowLog.IsChecked == true)
                Editor.ScrollToEnd();
        }, DispatcherPriority.Loaded);
    }

    void FollowLog_OnClick(object? sender, RoutedEventArgs e)
    {
        if (FollowLog.IsChecked == true) CloseSearch();
        ScrollIfFollowing();
    }

    void Editor_OnKeyDown(object? sender, KeyEventArgs e)
    {
        var shortcut = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
        if (e.Key == Key.F && e.KeyModifiers == shortcut)
        {
            OpenSearch();
            e.Handled = true;
        }
    }

    void FindLog_OnClick(object? sender, RoutedEventArgs e)
    {
        if (search is { IsOpened: true }) CloseSearch();
        else OpenSearch();
    }

    void OpenSearch()
    {
        FollowLog.IsChecked = false;
        if (search == null)
        {
            search = SearchPanel.Install(Editor);
            search.TemplateApplied += Search_TemplateApplied;
        }
        search.Open();
        FindLog.IsEnabled = true;
        // The search text box is created when the panel is attached.
        Dispatcher.UIThread.Post(() =>
        {
            if (search.IsOpened) search.Reactivate();
        }, DispatcherPriority.Loaded);
    }

    void Search_TemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        var panel = (SearchPanel)sender!;
        foreach (var child in panel.GetTemplateChildren())
        {
            if (child is not Button button || button.Command != SearchCommands.CloseSearchPanel) continue;

            // The editor's routed command depends on focus; the close button owns this action.
            button.Command = null;
            button.Click += SearchClose_OnClick;
            AutomationProperties.SetName(button, AppResources.Close);
            ToolTip.SetTip(button, AppResources.Close);
            break;
        }
    }

    void SearchClose_OnClick(object? sender, RoutedEventArgs e) => CloseSearch();

    void CloseSearch()
    {
        if (search is not { IsOpened: true }) return;
        search.Close();
        FindLog.IsEnabled = Editor.Document.TextLength != 0;
    }

    async void CopyLog_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (TopLevel.GetTopLevel(this)?.Clipboard is { } clipboard && Editor.Document.TextLength != 0)
                await clipboard.SetTextAsync(Editor.SelectionLength != 0 ? Editor.SelectedText : Editor.Document.Text);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
    }
}
