namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorPageViewModel
{
    bool borderbottomisactive;
    bool _hasPasswordEncrypt = false;
    bool _hasLocalPcEncrypt = false;
    bool _isVerificationPass;
    bool _authenticatorIsEmpty = true;
    Stream? _qrCodeStream;

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

    public bool AuthenticatorIsEmpty
    {
        get => _authenticatorIsEmpty;
        set
        {
            if (value == _authenticatorIsEmpty) return;
            _authenticatorIsEmpty = value;
            this.RaisePropertyChanged();
        }
    }

    public Stream? QrCodeStream
    {
        get => _qrCodeStream;
        set
        {
            if (value == _qrCodeStream) return;
            _qrCodeStream = value;
            this.RaisePropertyChanged();
        }
    }
}