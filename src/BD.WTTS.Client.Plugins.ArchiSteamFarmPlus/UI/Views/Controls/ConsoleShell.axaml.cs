using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace BD.WTTS.UI.Views.Controls;

public partial class ConsoleShell : UserControl
{
    public const string InputIndicatorSuffix = "> ";
    public const string OutputIndicatorPrefix = "< ";

    /// <summary>
    /// Defines the <see cref="InputIndicator"/> property.
    /// </summary>
    public static readonly StyledProperty<string> InputIndicatorProperty =
        AvaloniaProperty.Register<ConsoleShell, string>(nameof(InputIndicator), string.Empty);

    /// <summary>
    /// Defines the <see cref="IsMask"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMaskProperty =
        AvaloniaProperty.Register<ConsoleShell, bool>(nameof(IsMask), false);

    /// <summary>
    /// Defines the <see cref="LogText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> LogTextProperty =
        AvaloniaProperty.Register<ConsoleShell, string>(nameof(LogText), string.Empty);

    /// <summary>
    /// Defines the <see cref="MaxLine"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxLineProperty =
        AvaloniaProperty.Register<ConsoleShell, int>(nameof(MaxLine), 300);

    /// <summary>
    /// Defines the Avalonia.Controls.ItemsControl.Items property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> AutoCompleteBoxItemsProperty =
        ItemsControl.ItemsSourceProperty.AddOwner<ConsoleShell>();

    /// <summary>
    /// Defines the Avalonia.Controls.ItemsControl.ItemTemplate property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> AutoCompleteBoxItemTemplateProperty =
        AvaloniaProperty.Register<ConsoleShell, IDataTemplate>(nameof(AutoCompleteBoxItemTemplate));

    private IEnumerable _autoCompleteBoxItems = new AvaloniaList<object>();

    /// <summary>
    ///  Gets or sets the items to display.
    /// </summary>
    public IEnumerable AutoCompleteBoxItems
    {
        get
        {
            return _autoCompleteBoxItems;
        }

        set
        {
            SetValue(AutoCompleteBoxItemsProperty, value);
        }
    }

    /// <summary>
    /// Gets or sets the data template used to display the items in the control.
    /// </summary>
    public IDataTemplate AutoCompleteBoxItemTemplate
    {
        get
        {
            return GetValue(AutoCompleteBoxItemTemplateProperty);
        }
        set => SetValue(AutoCompleteBoxItemTemplateProperty, value);
    }

    /// <summary>
    /// 输入指示器
    /// </summary>
    public string InputIndicator
    {
        get => GetValue(InputIndicatorProperty);
        set => SetValue(InputIndicatorProperty, value);
    }

    /// <summary>
    /// 隐藏指令内容（用于密码输入）
    /// </summary>
    public bool IsMask
    {
        get => GetValue(IsMaskProperty);
        set => SetValue(IsMaskProperty, value);
    }

    /// <summary>
    /// 历史日志内容
    /// </summary>
    public string LogText
    {
        get => GetValue(LogTextProperty);
        set => SetValue(LogTextProperty, value);
    }

    /// <summary>
    /// 最大日志行数
    /// </summary>
    public int MaxLine
    {
        get => GetValue(MaxLineProperty);
        set => SetValue(MaxLineProperty, value);
    }

    private CommandEventArgs? _commandSubmitArgs;

    /// <summary>
    /// 输入命令后按下回车触发
    /// </summary>
    public event EventHandler<CommandEventArgs>? CommandSubmit;

    private readonly TextBlock inputIndicatorTextBlock;
    private readonly AutoCompleteBox commandTextbox;
    private readonly TextBox logTextbox;
    private readonly ScrollViewer consoleScroll;
    private readonly ArchiSteamFarmCommandHistory commandHistory;

    /// <summary>
    /// 无参构造
    /// </summary>
    public ConsoleShell()
    {
        InitializeComponent();

        commandHistory = new();
        inputIndicatorTextBlock = this.FindControl<TextBlock>("InputIndicatorTextBlock")!;
        logTextbox = this.FindControl<TextBox>("LogTextbox")!;
        consoleScroll = this.FindControl<ScrollViewer>("ConsoleScroll")!;
        commandTextbox = this.FindControl<AutoCompleteBox>("CommandTextbox")!;
        commandTextbox.KeyUp += CommandTextbox_KeyUp;

        commandTextbox[!AutoCompleteBox.ItemsSourceProperty] = this[!AutoCompleteBoxItemsProperty];
        commandTextbox[!AutoCompleteBox.ItemTemplateProperty] = this[!AutoCompleteBoxItemTemplateProperty];

        commandTextbox.GetObservable(AutoCompleteBox.TextProperty)
            .Subscribe(x =>
            {
                if (!string.IsNullOrEmpty(x))
                {
                    var i = x.IndexOf(Environment.NewLine);
                    if (i >= 1)
                    {
                        //OnCommandSubmit(x.Substring(0, i));
                        x = x[..i];
                    }
                }
            });

        this.GetObservable(InputIndicatorProperty)
            .Subscribe(x => inputIndicatorTextBlock.Text = $"{InputIndicator}{InputIndicatorSuffix}");

        this.GetObservable(IsMaskProperty)
            .Subscribe(x => ((IPseudoClasses)commandTextbox.Classes).Set(":password", x));

        this.GetObservable(LogTextProperty)
            .Subscribe(x =>
            {
                if (!string.IsNullOrEmpty(LogText) && !logTextbox.IsVisible)
                {
                    logTextbox.IsVisible = true;
                }
                //logTextbox.Text = x;

                if (logTextbox.Width > 0)
                    logTextbox.MaxLength = (int)(logTextbox.Width / (logTextbox.FontSize + logTextbox.LetterSpacing)) * logTextbox.MaxLines;

                commandTextbox.Focus();
                consoleScroll.ScrollToEnd();
            });

        this.GetObservable(MaxLineProperty)
            .Subscribe(x => logTextbox.MaxHeight = (logTextbox.LineHeight = logTextbox.FontSize) * (logTextbox.MaxLines = x));

        this.GetObservable(FontSizeProperty).Subscribe(x => logTextbox.MaxHeight = (logTextbox.LineHeight = logTextbox.FontSize = x) * logTextbox.MaxLines);
    }

    private void CommandTextbox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Avalonia.Input.Key.Up: // Command history previous
                if (string.IsNullOrEmpty(commandHistory.CurrentCommand))
                {
                    commandHistory.CurrentCommand = commandTextbox.Text;
                }
                commandTextbox.Text = commandHistory.GetPrevious();
                break;
            case Avalonia.Input.Key.Down: // Command history next
                if (string.IsNullOrEmpty(commandHistory.CurrentCommand))
                {
                    commandHistory.CurrentCommand = commandTextbox.Text;
                }
                commandTextbox.Text = commandHistory.GetNext();
                break;
            case Avalonia.Input.Key.Enter: // Submit Command
                if (!string.IsNullOrEmpty(commandTextbox.Text))
                {
                    OnCommandSubmit(commandTextbox.Text);
                    commandHistory.AddCommandToHistory(commandTextbox.Text);
                    commandHistory.CurrentIndex = -1;
                    commandHistory.CurrentCommand = string.Empty;
                    commandTextbox.Text = string.Empty;
                }
                break;
            case Avalonia.Input.Key.Escape: // Command Clear
                commandTextbox.Text = commandHistory.CurrentCommand;
                commandHistory.CurrentCommand = string.Empty;
                commandHistory.CurrentIndex = -1;
                break;
            default:
                break;
        }
    }

    internal void OnCommandSubmit(string command)
    {
        if (CommandSubmit != null)
        {
            if (_commandSubmitArgs == null)
            {
                _commandSubmitArgs = new CommandEventArgs();
            }
            _commandSubmitArgs.Command = command;
            CommandSubmit(this, _commandSubmitArgs);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

public class CommandEventArgs : EventArgs
{
    public string? Command { get; set; }
}
