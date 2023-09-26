using Avalonia;
using Avalonia.Android;

namespace BD.WTTS.Droid.UI.Activities;

[Register(AssemblyInfo.JavaPkgNames.Activities + nameof(MainActivity))]
[Activity(Label = "@string/app_name", MainLauncher = true)]
sealed class MainActivity : AvaloniaMainActivity<App>
{
    protected sealed override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder);
    }
}