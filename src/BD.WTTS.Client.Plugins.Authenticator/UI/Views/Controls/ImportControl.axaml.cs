using Avalonia.Controls;

namespace BD.WTTS.UI.Views;
public partial class ImportControl : UserControl
{
    public ImportControl()
    {
        InitializeComponent();
        DataContext = new SteamImportAuthenticator();
    }
}
