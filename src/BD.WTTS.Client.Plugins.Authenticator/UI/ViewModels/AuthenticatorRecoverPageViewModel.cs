namespace BD.WTTS.UI.ViewModels;

public class AuthenticatorRecoverPageViewModel : ViewModelBase
{
    [Reactive]
    public ObservableCollection<AuthenticatorRecoverModel> AuthenticatorDeleteBackups { get; set; }

    readonly string _currentAnswer;

    public AuthenticatorRecoverPageViewModel()
    {
        AuthenticatorDeleteBackups = new();
        Initialize();
        _currentAnswer = string.Empty;
    }

    public AuthenticatorRecoverPageViewModel(string answer)
    {
        AuthenticatorDeleteBackups = new();
        Initialize();
        _currentAnswer = answer;
    }

    async void Initialize()
    {
        AuthenticatorDeleteBackups.Clear();
        var response =
            await IMicroServiceClient.Instance.AuthenticatorClient.GetAuthenticatorDeleteBackups();
        if (!response.IsSuccess || response.Content == null) return;

        foreach (var item in response.Content)
        {
            if (!item.IsRecovered)
                AuthenticatorDeleteBackups.Add(new(item));
        }
    }

    public async Task Recover()
    {
        var list = AuthenticatorDeleteBackups.Where(a => a.IsSelected).ToList();
        var ids = list.Select(a => a.AuthenticatorDeleteBackup.Id).ToArray();
        var response = await IMicroServiceClient.Instance.AuthenticatorClient.RecoverAuthenticatorsFromDeleteBackups(new()
        {
            Answer = _currentAnswer,
            Id = ids,
        });
        if (!response.IsSuccess) return;
        AuthenticatorDeleteBackups.Remove(list);
        Toast.Show(ToastIcon.Success, Strings.Auth_Sync_RecoverSuccess);
    }
}