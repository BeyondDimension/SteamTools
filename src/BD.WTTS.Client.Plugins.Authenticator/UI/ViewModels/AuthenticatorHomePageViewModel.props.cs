using KeyValuePair = BD.Common.Entities.KeyValuePair;

namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorHomePageViewModel
{
    [Reactive]
    public bool IsLoading { get; set; }

    [Reactive]
    public ObservableCollection<AuthenticatorItemModel> Auths { get; set; }

    [Reactive]
    public AuthenticatorItemModel? SelectedAuth { get; set; }

    [Reactive]
    public bool HasPasswordEncrypt { get; set; } = false;

    [Reactive]
    public bool HasLocalPcEncrypt { get; set; } = false;

    [Reactive]
    public bool IsVerificationPass { get; set; }
}