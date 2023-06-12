using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Splat;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorPageViewModel : TabItemViewModel
{
    public override string Name => "AuthenticatorPage";
    
    [Reactive]
    public ObservableCollection<AuthenticatorItemModel> Auths { get; set; }

    public AuthenticatorPageViewModel()
    {
        Auths = new();
        Start();
    }

    public async void Start()
    {
        var list = await AuthenticatorService.GetAllAuthenticatorsAsync();
        foreach (var item in list)
        {
            Auths.Add(new AuthenticatorItemModel(item));
        }
    }
}
