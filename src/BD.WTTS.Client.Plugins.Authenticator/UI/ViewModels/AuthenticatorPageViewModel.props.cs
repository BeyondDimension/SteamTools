namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorPageViewModel
{
    bool borderbottomisactive;
    bool _hasPasswordEncrypt;
    bool _hasLocalPcEncrypt;
    string? _password;
    string? _verifypassword;
    bool _isVerificationPass;

    int AuthenticatorId { get; set; } = -1;
    
    [Reactive]
    public ObservableCollection<AuthenticatorItemModel> Auths { get; set; }

    public bool BorderBottomIsActive
    {
        get => borderbottomisactive;
        set
        {
            this.RaiseAndSetIfChanged(ref borderbottomisactive, value);
        }
    }
    
    AuthenticatorItemModel? CurrentSelectedAuth
    {
        get
        {
            if (Auths.Count > 0 && AuthenticatorId > 0)
            {
                return Auths.First(i => i.AuthData.Id == AuthenticatorId);
            }
            return null;
        }
    }

    public bool HasPasswordEncrypt
    {
        get => _hasPasswordEncrypt;
        set
        {
            if (value == _hasPasswordEncrypt) return;
            _hasPasswordEncrypt = value;
            this.RaisePropertyChanged();
        }
    }

    public bool HasLocalPcEncrypt
    {
        get => _hasLocalPcEncrypt;
        set
        {
            if (value == _hasLocalPcEncrypt) return;
            _hasLocalPcEncrypt = value;
            this.RaisePropertyChanged();
        }
    }
    
    public string? Password
    {
        get => _password;
        set
        {
            if (value == _password) return;
            _password = value;
            this.RaisePropertyChanged();
        }
    }

    public string? VerifyPassword
    {
        get => _verifypassword;
        set
        {
            if (value == _verifypassword) return;
            _verifypassword = value;
            this.RaisePropertyChanged();
        }
    }

    public bool IsVerificationPass
    {
        get => _isVerificationPass;
        set
        {
            if (value == _isVerificationPass) return;
            _isVerificationPass = value;
            this.RaisePropertyChanged();
        }
    }
}