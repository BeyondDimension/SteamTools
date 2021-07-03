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

        ///// <summary>
        ///// Defines the <see cref="LogTexts"/> property.
        ///// </summary>
        //public static readonly StyledProperty<IEnumerable?> LogTextsProperty =
        //    AvaloniaProperty.Register<AvaloniaShell, IEnumerable?>(nameof(LogTexts), null);

        ///// <summary>
        ///// 输出日志字符串集合
        ///// </summary>
        //public IEnumerable? LogTexts
        //{
        //    get => GetValue(LogTextsProperty);
        //    set => SetValue(LogTextsProperty, value);
        //}

        public StringBuilder LogText { get; set; } = new StringBuilder();

        private CommandEventArgs? _commandSubmitArgs;
        /// <summary>
        /// 输入命令后按下回车触发
        /// </summary>
        public event EventHandler<CommandEventArgs>? CommandSubmit;


        private TextBox commandTextbox;
        private TextBox logTextbox;

        public ConsoleShell()
        {
            InitializeComponent();

            logTextbox = this.FindControl<TextBox>("LogTextbox");

            commandTextbox = this.FindControl<TextBox>("CommandTextbox");
            commandTextbox.KeyUp += CommandTextbox_KeyUp;

            //this.WhenAnyValue(x=>x.LogTexts)
            //    .Subscribe(x=>x.);
        }

        private void CommandTextbox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
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
                else
                {
                    _commandSubmitArgs.Command = command;
                }
                CommandSubmit(this, _commandSubmitArgs);
            }
        }

        public void AppendLogText(string text)
        {
            LogText.AppendLine(OutputIndicator + text);
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
