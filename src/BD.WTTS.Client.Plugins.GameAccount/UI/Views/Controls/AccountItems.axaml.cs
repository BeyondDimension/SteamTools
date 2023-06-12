using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Controls;

public partial class AccountItems : UserControl
{
    public AccountItems()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
            Design.SetDataContext(this, new PlatformAccount(ThirdpartyPlatform.Steam)
            {
                FullName = "Steam",
                Icon = "Steam"
            });
#endif
    }
}
