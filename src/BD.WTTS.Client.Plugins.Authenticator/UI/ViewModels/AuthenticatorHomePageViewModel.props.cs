using KeyValuePair = BD.Common.Entities.KeyValuePair;

namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorHomePageViewModel : TabItemViewModel
{
    public override string Name => Strings.LocalAuth;

    [Reactive]
    public bool IsLoading { get; set; }

    private SourceCache<AuthenticatorItemModel, ushort> AuthSource;

    private readonly ReadOnlyObservableCollection<AuthenticatorItemModel> _Auths;

    public ReadOnlyObservableCollection<AuthenticatorItemModel> Auths => _Auths;

    //[Reactive]
    //public ObservableCollection<AuthenticatorItemModel> Auths { get; set; } = [];

    //[Reactive]
    //public AuthenticatorItemModel? SelectedAuth { get; set; }

    [Reactive]
    public bool HasPasswordEncrypt { get; set; } = false;

    [Reactive]
    public bool HasLocalPcEncrypt { get; set; } = false;

    [Reactive]
    public bool IsVerificationPass { get; set; }

    [Reactive]
    public string? SearchText { get; set; }
}