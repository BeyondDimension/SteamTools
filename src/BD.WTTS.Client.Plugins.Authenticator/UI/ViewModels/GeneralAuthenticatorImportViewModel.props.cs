namespace BD.WTTS.UI.ViewModels;

public partial class GeneralAuthenticatorImportViewModel
{
    string? _authenticatorName;
    string? _secretCode;
    string? _currentCode;
    CanImportAuthenticatorType _importAuthenticatorType;

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
    
    public enum CanImportAuthenticatorType
    {
        谷歌令牌,
        微软令牌,
        其他令牌,
    }

    public CanImportAuthenticatorType ImportAuthenticatorType
    {
        get => _importAuthenticatorType;
        set
        {
            if (value == _importAuthenticatorType) return;
            _importAuthenticatorType = value;
            this.RaisePropertyChanged();
        }
    }

    public CanImportAuthenticatorType[] ImportAuthenticatorTypes => Enum.GetValues<CanImportAuthenticatorType>();
}