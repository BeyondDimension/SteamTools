using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;
public partial class DebugPage : ReactiveUserControl<DebugPageViewModel>
{
    public DebugPage()
    {
        InitializeComponent();

        CommandTextBox.KeyUp += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                var command = CommandTextBox.Text;
                CommandTextBox.Text = null;
                this.ViewModel?.Debug(command);
            }
        };
    }
}
