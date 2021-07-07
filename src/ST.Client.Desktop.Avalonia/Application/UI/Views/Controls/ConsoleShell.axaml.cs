using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Application.UI.Views.Controls
{
    public partial class ConsoleShell : UserControl
    {
        public const string InputIndicator = "> ";
        public const string OutputIndicator = "< ";

        /// <summary>
        /// Defines the <see cref="LogTexts"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsMaskProperty =
            AvaloniaProperty.Register<ConsoleShell, bool>(nameof(IsMask), false);

        /// <summary>
        /// 隐藏指令内容（用于密码输入）
        /// </summary>
        public bool IsMask
        {
            get => GetValue(IsMaskProperty);
            set => SetValue(IsMaskProperty, value);
        }

        public StringBuilder LogText { get; set; } = new StringBuilder();

        private CommandEventArgs? _commandSubmitArgs;
        /// <summary>
        /// 输入命令后按下回车触发
        /// </summary>
        public event EventHandler<CommandEventArgs>? CommandSubmit;

        private TextBox commandTextbox;
        private TextBox logTextbox;
        private ScrollViewer consoleScroll;

        public ConsoleShell()
        {
            InitializeComponent();

            logTextbox = this.FindControl<TextBox>("LogTextbox");
            consoleScroll = this.FindControl<ScrollViewer>("ConsoleScroll");
            commandTextbox = this.FindControl<TextBox>("CommandTextbox");
            commandTextbox.KeyUp += CommandTextbox_KeyUp;

            this.logTextbox.GetObservable(TextBox.TextProperty)
                  .Subscribe(x =>
                  {
                      if (this.logTextbox.IsVisible = !string.IsNullOrEmpty(x))
                      {
                          //if (x.EndsWith(Environment.NewLine))
                          //{
                          //    x = x.Remove(x.Length);
                          //}
                          consoleScroll.ScrollToEnd();
                          //consoleScroll.Offset = new Vector(double.NegativeInfinity, consoleScroll.Viewport.Height);
                      }
                  });

            this.GetObservable(IsMaskProperty)
                  .Subscribe(x => commandTextbox.PasswordChar = x ? '*' : default);
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
            LogText.AppendLine(text);
            logTextbox.Text = LogText.ToString();
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
