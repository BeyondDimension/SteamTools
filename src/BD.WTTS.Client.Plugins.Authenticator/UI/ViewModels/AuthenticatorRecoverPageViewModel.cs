namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorRecoverPageViewModel : ViewModelBase
{
    public ObservableCollection<AuthenticatorRecoverModel> AuthenticatorDeleteBackups { get; } = new();

    [Reactive]
    public partial bool IsLoading { get; private set; } = true;

    public ICommand RecoverCommand { get; }

    readonly string _currentAnswer;

    public AuthenticatorRecoverPageViewModel() : this(string.Empty)
    {
    }

    public AuthenticatorRecoverPageViewModel(string answer)
    {
        _currentAnswer = answer;
        var command = ReactiveCommand.CreateFromTask(Recover);
        CompositeDisposable.Add(command);
        CompositeDisposable.Add(command.ThrownExceptions.Subscribe(static ex => ex.LogAndShowT()));
        RecoverCommand = command;
        _ = InitializeAsync();
    }

    async Task InitializeAsync()
    {
        try
        {
            var response = await IMicroServiceClient.Instance.AuthenticatorClient.GetAuthenticatorDeleteBackups();
            if (Disposed || !response.IsSuccess || response.Content is null) return;

            foreach (var item in response.Content)
                if (!item.IsRecovered)
                    AuthenticatorDeleteBackups.Add(new(item));
        }
        catch (Exception ex)
        {
            if (!Disposed) ex.LogAndShowT();
        }
        finally
        {
            IsLoading = false;
        }
    }

    async Task Recover()
    {
        var list = AuthenticatorDeleteBackups.Where(a => a.IsSelected).ToList();
        if (list.Count == 0) return;
        var ids = list.Select(a => a.AuthenticatorDeleteBackup.Id).ToArray();
        var response = await IMicroServiceClient.Instance.AuthenticatorClient.RecoverAuthenticatorsFromDeleteBackups(new()
        {
            Answer = _currentAnswer,
            Id = ids,
        });
        if (Disposed || !response.IsSuccess) return;
        AuthenticatorDeleteBackups.Remove(list);
        Toast.Show(ToastIcon.Success, Strings.Auth_Sync_RecoverSuccess);
    }
}