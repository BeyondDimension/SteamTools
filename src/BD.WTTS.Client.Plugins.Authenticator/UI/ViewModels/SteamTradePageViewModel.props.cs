using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel : ViewModelBase
{
    string? _userNameText;
    string? _passwordText;
    string? _captchaCodeText;
    string? _captchaImageUrlText;
    bool _isLogging;
    bool _isLogged;
    int _selectIndex;
    bool _remenberLogin;
    bool _isLoading;
    ReadOnlyObservableCollection<WinAuth.SteamClient.Confirmation>? _confirmations;
    bool _unSelectAll;
    string? _selectAllText;

    public string? UserNameText
    {
        get => _userNameText;
        set
        {
            if (value == _userNameText) return;
            _userNameText = value;
            this.RaisePropertyChanged();
        }
    }

    public string? PasswordText
    {
        get => _passwordText;
        set
        {
            if (value == _passwordText) return;
            _passwordText = value;
            this.RaisePropertyChanged();
        }
    }

    public string? CaptchaImageUrlText
    {
        get => _captchaImageUrlText;
        set
        {
            if (value == _captchaImageUrlText) return;
            _captchaImageUrlText = value;
            this.RaisePropertyChanged();
        }
    }

    public string? CaptchaCodeText
    {
        get => _captchaCodeText;
        set
        {
            if (value == _captchaCodeText) return;
            _captchaCodeText = value;
            this.RaisePropertyChanged();
        }
    }

    public bool IsLogging
    {
        get => _isLogging;
        set
        {
            if (value == _isLogging) return;
            _isLogging = value;
            this.RaisePropertyChanged();
        }
    }

    public bool IsLogged
    {
        get => _isLogged;
        set
        {
            if (value == _isLogged) return;
            _isLogged = value;
            this.RaisePropertyChanged();
        }
    }

    public int SelectIndex
    {
        get => _selectIndex;
        set
        {
            if (value == _selectIndex) return;
            _selectIndex = value;
            this.RaisePropertyChanged();
        }
    }

    public bool RemenberLogin
    {
        get => _remenberLogin;
        set
        {
            if (value == _remenberLogin) return;
            _remenberLogin = value;
            this.RaisePropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (value == _isLoading) return;
            _isLoading = value;
            this.RaisePropertyChanged();
        }
    }

    public bool UnSelectAll
    {
        get => _unSelectAll;
        set
        {
            if (this.RaiseAndSetIfChangedReturnIsNotChange(ref _unSelectAll, value)) return;
            _isUnselectAllChangeing = true;
            foreach (var item in Confirmations)
            {
                if (item.IsOperate != 0) continue;
                item.NotChecked = value;
            }
            _isUnselectAllChangeing = false;
            SelectAllText = Strings.SelectAllText_.Format(value ? 0 : Confirmations.Count, Confirmations.Count);
        }
    }

    public string? SelectAllText
    {
        get => _selectAllText;
        set
        {
            if (value == _selectAllText) return;
            _selectAllText = value;
            this.RaisePropertyChanged();
        }
    }

    public ReadOnlyObservableCollection<WinAuth.SteamClient.Confirmation> Confirmations =>
        _confirmations ?? throw new ArgumentNullException(nameof(_confirmations));
}