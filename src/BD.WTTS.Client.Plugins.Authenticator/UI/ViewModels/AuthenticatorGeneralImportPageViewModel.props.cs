namespace BD.WTTS.UI.ViewModels;

public partial class AuthenticatorGeneralImportPageViewModel : ViewModelBase
{
    string? _authenticatorName;
    string? _secretCode;
    string? _currentCode;

    public string? AuthenticatorName
    {
        get => _authenticatorName;
        set
        {
            if (value == _authenticatorName) return;
            _authenticatorName = value;
            this.RaisePropertyChanged();
        }
    }

    public string? SecretCode
    {
        get => _secretCode;
        set
        {
            if (value == _secretCode) return;
            _secretCode = value;
            this.RaisePropertyChanged();
        }
    }

    public string? CurrentCode
    {
        get => _currentCode;
        set
        {
            if (value == _currentCode) return;
            _currentCode = value;
            this.RaisePropertyChanged();
        }
    }
}