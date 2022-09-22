#if ANDROID
using AndroidX.Fragment.App;
using AView = Android.Views.View;
using BaseShellRenderer = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;

namespace System.Application.UI;

public sealed partial class ShellRenderer : BaseShellRenderer
{
    protected override void SwitchFragment(FragmentManager manager, AView targetView, ShellItem newItem, bool animate)
    {
        // 禁用 Android 上的切换 Page 时动画
        base.SwitchFragment(manager, targetView, newItem, false);
    }
}
#endif