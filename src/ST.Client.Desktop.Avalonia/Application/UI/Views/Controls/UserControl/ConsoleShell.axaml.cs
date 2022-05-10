using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.UI.Views.Controls
{
    public partial class ConsoleShell : UserControl
    {
        public const string InputIndicator = "> ";
        public const string OutputIndicator = "< ";

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

        private readonly AutoCompleteBox commandTextbox;
        private readonly TextBox logTextbox;
        private readonly ScrollViewer consoleScroll;

        public ConsoleShell()
        {
            InitializeComponent();

            logTextbox = this.FindControl<TextBox>("LogTextbox");
            consoleScroll = this.FindControl<ScrollViewer>("ConsoleScroll");
            commandTextbox = this.FindControl<AutoCompleteBox>("CommandTextbox");
            commandTextbox.KeyUp += CommandTextbox_KeyUp;

            commandTextbox.GetObservable(AutoCompleteBox.TextProperty)
                .Subscribe(x =>
                {
                    if (!string.IsNullOrEmpty(x))
                    {
                        var i = x.IndexOf(Environment.NewLine);
                        if (i >= 1)
                        {
                            //OnCommandSubmit(x.Substring(0, i));
                            x = x.Substring(0, i);
                        }
                    }
                });

            //this.logTextbox.GetObservable(TextBox.TextProperty)
            //      .Subscribe(x =>
            //      {
            //          if (this.logTextbox.IsVisible = !string.IsNullOrEmpty(x))
            //          {
            //              consoleScroll.ScrollToEnd();
            //              //consoleScroll.Offset = new Vector(double.NegativeInfinity, consoleScroll.Viewport.Height);
            //          }
            //      });
            //this.GetObservable(IsMaskProperty)
            //      .Subscribe(x => commandTextbox.PasswordChar = x ? '●' : default);

            this.GetObservable(IsMaskProperty)
                  .Subscribe(x => ((IPseudoClasses)commandTextbox.Classes).Set(":password", x));

            this.GetObservable(LogTextProperty)
                  .Subscribe(x =>
                   {
                       if (!string.IsNullOrEmpty(LogText) && !logTextbox.IsVisible)
                       {
                           logTextbox.IsVisible = true;
                       }
                       logTextbox.Text = x;
                       consoleScroll.ScrollToEnd();
                       //consoleScroll.Offset += new Vector(0, 20);
                   });

            consoleScroll.AttachedToVisualTree += (s, e) =>
            {
                consoleScroll.ScrollToEnd();
            };
        }

        private void CommandTextbox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter && !string.IsNullOrEmpty(commandTextbox.Text))
            {
                OnCommandSubmit(commandTextbox.Text);
                commandTextbox.Text = string.Empty;
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

        public void AppendLogText(string text)
        {
            LogText += text;
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
}
