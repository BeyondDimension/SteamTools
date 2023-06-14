namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorPageViewModel
{
    bool borderbottomisactive;

    public bool BorderBottomIsActive
    {
        get => borderbottomisactive;
        set
        {
            this.RaiseAndSetIfChanged(ref borderbottomisactive, value);
        }
    }
}