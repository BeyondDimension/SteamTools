namespace BD.WTTS.UI.ViewModels;

public class MessageBoxWindowViewModel : DialogWindowViewModel
{
    string? _Content;

    public string? Content
    {
        get => _Content;
        set => this.RaiseAndSetIfChanged(ref _Content, value);
    }

    bool _IsCancelcBtn;

    public bool IsCancelcBtn
    {
        get => _IsCancelcBtn;
        set => this.RaiseAndSetIfChanged(ref _IsCancelcBtn, value);
    }

    bool _IsShowRememberChoose;

    public bool IsShowRememberChoose
    {
        get => _IsShowRememberChoose;
        set => this.RaiseAndSetIfChanged(ref _IsShowRememberChoose, value);
    }

    bool _RememberChoose;

    public bool RememberChoose
    {
        get => _RememberChoose;
        set => this.RaiseAndSetIfChanged(ref _RememberChoose, value);
    }
}
