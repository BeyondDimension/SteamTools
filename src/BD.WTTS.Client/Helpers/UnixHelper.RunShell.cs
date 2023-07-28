#if !WINDOWS
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class UnixHelper
{
    const string TAG = nameof(UnixHelper);

    /// <inheritdoc cref="IPlatformService.RunShellAsync(string, bool)"/>
    public static ValueTask RunShellAsync(string script, bool requiredAdministrator)
        => RunShellAsync(script, requiredAdministrator, 3);

    /// <inheritdoc cref="IPlatformService.RunShellAsync(string, bool)"/>
    static async ValueTask RunShellAsync(string script, bool requiredAdministrator, sbyte retry)
    {
        if (retry <= 0) return;
        var scriptContent = new StringBuilder();
        if (requiredAdministrator)
        {
            TextBoxWindowViewModel vm = new()
            {
                Title = AppResources.UnixSudoPasswordTips,
                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
                Description = $"sudo {script}",
            };
            await TextBoxWindowViewModel.ShowDialogAsync(vm);
            if (string.IsNullOrWhiteSpace(vm.Value))
                return;
            scriptContent.AppendLine($"echo \"{vm.Value}\" | sudo -S {script}");
        }
        scriptContent.AppendLine(script);
        var msg = RunShell(scriptContent.ToString());
        if (!string.IsNullOrWhiteSpace(msg))
            Toast.Show(ToastIcon.None, msg);
    }

    /// <summary>
    /// 执行脚本
    /// </summary>
    /// <param name="shell">脚本指令</param>
    /// <returns>返回结果</returns>
    public static string RunShell(string shell)
        => Process2.RunShell(Process2.BinBash, shell, e => e.LogAndShowT(TAG));
}
#endif