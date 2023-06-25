using Avalonia.Media.Imaging;

namespace BD.WTTS.UI.ViewModels;

public partial class SteamTradePageViewModel : INotifyPropertyChanged
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

    public string? UserNameText
    {
        get => _userNameText;
        set
        {
            if (value == _userNameText) return;
            _userNameText = value;
            OnPropertyChanged();
        }
    }

    public string? PasswordText
    {
        get => _passwordText;
        set
        {
            if (value == _passwordText) return;
            _passwordText = value;
            OnPropertyChanged();
        }
    }

    public string? CaptchaImageUrlText
    {
        get => _captchaImageUrlText;
        set
        {
            if (value == _captchaImageUrlText) return;
            _captchaImageUrlText = value;
            OnPropertyChanged();
        }
    }

    public string? CaptchaCodeText
    {
        get => _captchaCodeText;
        set
        {
            if (value == _captchaCodeText) return;
            _captchaCodeText = value;
            OnPropertyChanged();
        }
    }

    public bool IsLogging
    {
        get => _isLogging;
        set
        {
            if (value == _isLogging) return;
            _isLogging = value;
            OnPropertyChanged();
        }
    }

    public bool IsLogged
    {
        get => _isLogged;
        set
        {
            if (value == _isLogged) return;
            _isLogged = value;
            OnPropertyChanged();
        }
    }

    public int SelectIndex
    {
        get => _selectIndex;
        set
        {
            if (value == _selectIndex) return;
            _selectIndex = value;
            OnPropertyChanged();
        }
    }

    public bool RemenberLogin
    {
        get => _remenberLogin;
        set
        {
            if (value == _remenberLogin) return;
            _remenberLogin = value;
            OnPropertyChanged();
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (value == _isLoading) return;
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}