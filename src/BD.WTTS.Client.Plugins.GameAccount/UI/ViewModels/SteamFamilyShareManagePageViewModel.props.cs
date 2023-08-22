using BD.WTTS.Client.Resources;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamFamilyShareManagePageViewModel : WindowViewModel
{
    public ICommand OpenUserProfileUrl_Click { get; }

    public ICommand RemoveButton_Click { get; }

    //public ICommand SetFirstButton_Click { get; }

    //public ICommand UpButton_Click { get; }

    //public ICommand DownButton_Click { get; }
}
