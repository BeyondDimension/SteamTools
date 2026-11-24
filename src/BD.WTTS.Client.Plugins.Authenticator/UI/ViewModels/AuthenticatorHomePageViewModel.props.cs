using KeyValuePair = BD.Common.Entities.KeyValuePair;

namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorHomePageViewModel : TabItemViewModel
{
    public override string Name => Strings.LocalAuth;

    [Reactive]
    public partial bool IsLoading { get; set; }

    private SourceCache<AuthenticatorItemModel, ushort> AuthSource;

    private readonly ReadOnlyObservableCollection<AuthenticatorItemModel> _Auths;

    public ReadOnlyObservableCollection<AuthenticatorItemModel> Auths => _Auths;

    //[Reactive]
    //public ObservableCollection<AuthenticatorItemModel> Auths { get; set; } = [];

    //[Reactive]
    //public AuthenticatorItemModel? SelectedAuth { get; set; }

    [Reactive]
    public partial bool HasPasswordEncrypt { get; private set; }

    [Reactive]
    public partial bool HasLocalPcEncrypt { get; private set; }

    [Reactive]
    public partial bool IsVerificationPass { get; private set; }

    [Reactive]
    public partial string? SearchText { get; set; }
}