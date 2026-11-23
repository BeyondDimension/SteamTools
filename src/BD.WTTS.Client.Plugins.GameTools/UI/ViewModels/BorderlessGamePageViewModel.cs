using ReactiveUI.Reactive;
using System.Windows.Input;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class BorderlessGamePageViewModel : ViewModelBase
{
    #region SelectWindow 变更通知

    private NativeWindowModel? _SelectWindow;

    public NativeWindowModel? SelectWindow
    {
        get => _SelectWindow;
        set
        {
            if (_SelectWindow != value)
            {
                _SelectWindow = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(HasSelectedWindow));
            }
        }
    }

    #endregion

    ObservableCollection<NativeWindowModel> _WindowList = new();

    public ObservableCollection<NativeWindowModel> WindowList
    {
        get => _WindowList;
        set
        {
            if (_WindowList != value)
            {
                _WindowList = value;
                this.RaisePropertyChanged();
            }
        }
    }

    public bool IsWindowPickingSupported { get; }

    public bool HasSelectedWindow => IsWindowPickingSupported && SelectWindow is not null;

    public ICommand? PickWindowCommand { get; }
    public ICommand? CopyGameInfoCommand { get; }
    public ICommand? BorderlessWindowCommand { get; }
    public ICommand? MaximizeWindowCommand { get; }
    public ICommand? NormalWindowCommand { get; }
    public ICommand? KillWindowCommand { get; }
    public ICommand? ShowWindowCommand { get; }
    public ICommand? HideWindowCommand { get; }
    public ICommand? ToWallpaperWindowCommand { get; }
    public ICommand? ResetWallpaperCommand { get; }

    public BorderlessGamePageViewModel()
    {
#if WINDOWS
        IsWindowPickingSupported = true;
        PickWindowCommand = Register(PickWindow);
        BorderlessWindowCommand = Register(BorderlessWindow);
        MaximizeWindowCommand = Register(MaximizeWindow);
        NormalWindowCommand = Register(NormalWindow);
        KillWindowCommand = Register(KillWindow);
        ShowWindowCommand = Register(ShowWindow);
        HideWindowCommand = Register(HideWindow);
        ToWallpaperWindowCommand = Register(ToWallpaperWindow);
        ResetWallpaperCommand = Register(ResetWallpaper);
        var copy = ReactiveCommand.CreateFromTask(CopyGameInfoAsync);
        CompositeDisposable.Add(copy);
        CopyGameInfoCommand = copy;
#endif
    }

#if WINDOWS
    readonly IPlatformService windowApi = IPlatformService.Instance!;

    ICommand Register(Action action)
    {
        var command = ReactiveCommand.Create(action);
        CompositeDisposable.Add(command);
        return command;
    }

    async Task CopyGameInfoAsync()
    {
        if (SelectWindow is not { } window)
            return;

        var text = $"{Strings.GameRelated_Borderless_Title}: {window.Title}\n" +
                   $"{Strings.GameRelated_Borderless_Process}: {window.Name}\n" +
                   $"{Strings.GameRelated_Borderless_ClassName}: {window.ClassName}\n" +
                   $"{Strings.GameRelated_Borderless_Path}: {window.Path}";
        await Clipboard2.SetTextAsync(text);
        Toast.Show(ToastIcon.Success, Strings.CopyToClipboard);
    }

    void PickWindow()
    {
        windowApi.GetMoveMouseDownWindow((window) =>
        {
            if (window is not null)
                WindowList.Insert(0, window);
            SelectWindow = window;
        });
    }

    void BorderlessWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.BorderlessWindow(SelectWindow);
    }

    void MaximizeWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.MaximizeWindow(SelectWindow);
    }

    void NormalWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.NormalWindow(SelectWindow);
    }

    void KillWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        SelectWindow.Kill();
    }

    void ShowWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.ShowWindow(SelectWindow);
    }

    void HideWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.HideWindow(SelectWindow);
    }

    void ToWallpaperWindow()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.ToWallerpaperWindow(SelectWindow);
    }

    void ResetWallpaper()
    {
        windowApi.ResetWallerpaper();
    }
#endif
}
