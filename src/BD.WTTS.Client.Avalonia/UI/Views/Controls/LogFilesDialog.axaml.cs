using BD.WTTS.Client.Resources;
using BD.WTTS.Logging;

namespace BD.WTTS.UI.Views.Controls;

public partial class LogFilesDialog : UserControl
{
    CancellationTokenSource? pendingWork;
    bool updatingFiles;
    bool hadDiscoveryError;
    string? logDirectory;

    public LogFilesDialog()
    {
        InitializeComponent();
    }

    /// <summary>Show either the application log collection or one known log directory.</summary>
    public static async Task ShowAsync(Control owner, string? logDirectory = null, string? title = null)
    {
        if (TopLevel.GetTopLevel(owner) is not { } root)
            return;

        var view = new LogFilesDialog
        {
            logDirectory = logDirectory,
            Width = Math.Clamp(root.Bounds.Width - 96, 240, 640),
            Height = Math.Clamp(root.Bounds.Height - 160, 140, 460),
        };
        var dialog = new TaskDialogEx
        {
            Title = title ?? Strings.LogConsole_ViewLogs,
            Header = title ?? Strings.LogConsole_ViewLogs,
            Content = view,
            XamlRoot = root,
        };
        dialog.Classes.Add("disableScroll");
        dialog.Buttons.Add(new TaskDialogButton(Strings.Close, TaskDialogStandardResult.Close));
        try
        {
            await dialog.ShowAsync(true);
        }
        finally
        {
            view.CancelPendingWork();
        }
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        await RefreshAsync();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        CancelPendingWork();
        base.OnUnloaded(e);
    }

    void CancelPendingWork()
    {
        var previous = pendingWork;
        pendingWork = null;
        previous?.Cancel();
    }

    async void Refresh_Click(object? sender, RoutedEventArgs e) => await RefreshAsync();

    async void LogFile_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!updatingFiles && LogFiles.SelectedItem is LogFileReader.LogFile file)
            await RunAsync(token => DisplayFileAsync(file, token));
    }

    Task RefreshAsync() => RunAsync(async token =>
    {
        var previousPath = (LogFiles.SelectedItem as LogFileReader.LogFile)?.DisplayPath;
        LogFiles.IsEnabled = false;
        string? asfLogRoot = null;
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        if (logDirectory == null)
            asfLogRoot = IApplication.LogDirPathASF;
#endif
        var (files, hadReadError) = await LogFileReader.DiscoverAsync(logDirectory ?? IApplication.LogDirPath, asfLogRoot,
            token, includeModuleDirectories: logDirectory == null);
        token.ThrowIfCancellationRequested();
        hadDiscoveryError = hadReadError;
        var selected = files.FirstOrDefault(file => file.DisplayPath == previousPath) ?? files.FirstOrDefault();
        updatingFiles = true;
        try
        {
            LogFiles.ItemsSource = files;
            LogFiles.SelectedItem = selected;
            LogFiles.IsEnabled = files.Length > 0;
        }
        finally
        {
            updatingFiles = false;
        }

        if (selected is null)
            LogStatus.Text = hadReadError ? Strings.LogConsole_ReadError : Strings.LogConsole_NoFiles;
        else
            await DisplayFileAsync(selected, token);
    });

    async Task RunAsync(Func<CancellationToken, Task> operation)
    {
        CancelPendingWork();
        using var request = new CancellationTokenSource();
        pendingWork = request;
        LogOutput.ResetLog();
        LogStatus.Text = Strings.Loading;
        try
        {
            await operation(request.Token);
        }
        catch (OperationCanceledException) when (request.IsCancellationRequested)
        {
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            if (!request.IsCancellationRequested)
                LogStatus.Text = Strings.LogConsole_ReadError;
        }
        finally
        {
            if (ReferenceEquals(pendingWork, request))
                pendingWork = null;
        }
    }

    async Task DisplayFileAsync(LogFileReader.LogFile file, CancellationToken cancellationToken)
    {
        var (text, isTail) = await LogFileReader.ReadTailAsync(file, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        LogOutput.WriteText(text);
        LogStatus.Text = text.Length == 0 ? Strings.LogConsole_EmptyFile : isTail
            ? string.Format(Strings.LogConsole_FileTail, LogFileReader.MaxTailBytes)
            : string.Empty;
        if (hadDiscoveryError)
            LogStatus.Text = string.IsNullOrEmpty(LogStatus.Text)
                ? Strings.LogConsole_ReadError
                : LogStatus.Text + " " + Strings.LogConsole_ReadError;
    }
}
