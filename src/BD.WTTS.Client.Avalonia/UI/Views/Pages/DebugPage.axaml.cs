using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class DebugPage : PageBase<DebugPageViewModel>
{
    public DebugPage()
    {
        InitializeComponent();
        this.SetViewModel<DebugPageViewModel>(false);

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
