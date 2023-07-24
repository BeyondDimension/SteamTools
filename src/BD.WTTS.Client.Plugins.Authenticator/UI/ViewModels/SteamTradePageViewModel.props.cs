using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel : ViewModelBase
{
    string? _userNameText;
    string? _passwordText;
    string? _captchaCodeText;
    string? _captchaImageUrlText;
    bool _isLogged;
    bool _isLoading;
    bool _selectedAll;

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

    public bool SelectedAll
    {
        get => _selectedAll;
        set
        {
            if (value == _selectedAll) return;
            _selectedAll = value;
            SelectAll(_selectedAll);
            this.RaisePropertyChanged();
        }
    }

    public bool IsConfirmationsAny => Confirmations.Any_Nullable();
    
}