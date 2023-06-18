using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Controls;

public partial class AccountItems : ReactiveUserControl<PlatformAccount>
{
    public AccountItems()
    {
        InitializeComponent();

        //this.WhenActivated(disposable =>
        //{
        //    ViewModel?.LoadUsers();
        //});

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
