#if MACOS || MACCATALYST || IOS
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    internal static async ValueTask RunShellCoreAsync(string script, bool requiredAdministrator)
    {
        var scriptContent = new StringBuilder();
        if (requiredAdministrator)
        {
            TextBoxWindowViewModel vm = new()
            {
                Title = AppResources.MacSudoPasswordTips,
                InputType = TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText,
                Description = $"sudo {script}",
            };
            if (await TextBoxWindowViewModel.ShowDialogAsync(vm) == null)
                return;
            scriptContent.AppendLine($"osascript -e 'tell app \"Terminal\" to do script \"sudo -S {script}\"'");
        }
        else
        {
            scriptContent.AppendLine(script);
        }
        var msg = UnixHelper.RunShell(scriptContent.ToString());
        if (!string.IsNullOrWhiteSpace(msg))
        {
            Toast.Show(ToastIcon.None, msg);
        }
    }

    /// <summary>
    /// 运行 Shell 脚本
    /// </summary>
    /// <param name="script">要运行的脚本字符串</param>
    /// <param name="requiredAdministrator">是否以管理员或 Root 权限运行</param>
    public static async void RunShell(string script, bool requiredAdministrator = false)
         => await RunShellAsync(script, requiredAdministrator);

    /// <inheritdoc cref="RunShell(string, bool)"/>
    public static ValueTask RunShellAsync(string script, bool requiredAdministrator = false)
    {
        return MacCatalystPlatformServiceImpl.RunShellCoreAsync(script, requiredAdministrator);
    }
}
#endif