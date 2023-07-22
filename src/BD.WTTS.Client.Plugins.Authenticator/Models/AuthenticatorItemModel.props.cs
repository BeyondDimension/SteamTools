using Avalonia.Media;

namespace BD.WTTS.Models;

public partial class AuthenticatorItemModel
{
    bool _isSelected;
    string _authName;
    string? _text;
    double _value;
    IBrush? _strokeColor;
    bool _isShowCode;

    public override bool IsSelected
    {
        get => _isSelected;
        set
        {
            this.RaiseAndSetIfChanged(ref _isSelected, value);
            IsShowCode = value;
            if (IsShowCode)
            {
                EnableShowCode();
            }
            else
            {
                DisableShowCode();
            }
            OnAuthenticatorItemIsSelectedChanged?.Invoke(this);
        }
    }

    public bool IsCloudAuth => AuthData.ServerId != null;

    public string AuthName
    {
        get => _authName;
        set => this.RaiseAndSetIfChanged(ref _authName, value);
    }

    public string? Text
    {
        get => _text;
        set => this.RaiseAndSetIfChanged(ref _text, value);
    }

    public IBrush? StrokeColor
    {
        get => _strokeColor;
        set => this.RaiseAndSetIfChanged(ref _strokeColor, value);
    }
    
    public double Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, (double)(value * 12.00d));
    }

    public bool IsShowCode
    {
        get => _isShowCode;
        set
        {
            if (_isShowCode == value) return;
            this.RaiseAndSetIfChanged(ref _isShowCode, value);
        }
    }

    public delegate void AuthenticatorItemIsSelectedChangeEventHandler(AuthenticatorItemModel sender);

    public static event AuthenticatorItemIsSelectedChangeEventHandler? OnAuthenticatorItemIsSelectedChanged;
}