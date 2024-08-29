using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using Org.BouncyCastle.Math;
using TextMateSharp.Grammars;

namespace BD.WTTS.UI.Views.Controls;

public partial class ProxyLog : UserControl
{
    CancellationTokenSource cancellation = new();

    public ProxyLog()
    {
        InitializeComponent();

        var logTextbox = this.FindControl<TextEditor>("LogTextbox")!;

        //Here we initialize RegistryOptions with the theme we want to use.
        var registryOptions = new TextMateSharp.Grammars.RegistryOptions(ThemeName.Dark);

        //Initial setup of TextMate.
        var textMateInstallation = logTextbox.InstallTextMate(registryOptions);

        //Here we are getting the language by the extension and right after that we are initializing grammar with this language.
        //And that's all 😀, you are ready to use AvaloniaEdit with syntax highlighting!
        textMateInstallation.SetGrammar(registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".log").Id));

        //logTextbox.Text = LogText;
        //logTextbox.ScrollToLine(logTextbox.Document.LineCount);

        logTextbox.TextChanged += LogTextbox_TextChanged;

        ProxyService.Current.WhenAnyValue(x => x.ProxyStatus)
                    .Subscribe(x =>
                    {
                        if (x)
                        {
                            cancellation = new CancellationTokenSource();
                            Task2.InBackground(FlushLogsAsync, true);
                        }
                        else
                        {
                            if (cancellation != null)
                            {
                                cancellation.Cancel();
                                cancellation.Dispose();
                            }
                        }
                    });
    }

    private void LogTextbox_TextChanged(object? sender, EventArgs e)
    {
        LogTextbox.ScrollToLine(LogTextbox.Document.LineCount);
    }

    void FlushLogsAsync()
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                var isAttachedToVisualTree = this.IsAttachedToVisualTree();
                if (isAttachedToVisualTree)
                {
                    var logtext = IReverseProxyService.Constants.Instance.GetLogAllMessage();
                    if (string.IsNullOrEmpty(logtext))
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    Dispatcher.UIThread.Post(() =>
                    {
                        if (LogTextbox.Text.Length != logtext.Length)
                            LogTextbox.Text = logtext;
                    });
                }
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
}