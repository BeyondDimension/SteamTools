using Avalonia.Platform;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class BorderlessGamePageViewModel : ViewModelBase
{
#if WINDOWS
    readonly IPlatformService windowApi = IPlatformService.Instance!;

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

    public BorderlessGamePageViewModel()
    {

    }

    public void Cross_MouseDown()
    {
        windowApi.GetMoveMouseDownWindow((window) =>
        {
            SelectWindow = window;
            WindowList.Insert(0, window);
        });
    }

    public void BorderlessWindow_Click()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.BorderlessWindow(SelectWindow);
    }

    public void MaximizeWindow_Click()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.MaximizeWindow(SelectWindow);
    }

    public void NormalWindow_Click()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.NormalWindow(SelectWindow);
    }

    public void WindowKill_Click()
    {
        if (SelectWindow == null)
        {
            return;
        }
        SelectWindow.Kill();
    }

    public void ShowWindow_Click()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.ShowWindow(SelectWindow);
    }

    public void HideWindow_Click()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.HideWindow(SelectWindow);
    }

    public void ToWallerpaperWindow_Click()
    {
        if (SelectWindow == null)
        {
            return;
        }
        windowApi.ToWallerpaperWindow(SelectWindow);
    }

    public void ResetWallerpaper_Click()
    {
        windowApi.ResetWallerpaper();
    }
#endif
}
