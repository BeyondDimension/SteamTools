using System;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

static partial class UnixHelper
{
    const string TAG = nameof(UnixHelper);

    /// <inheritdoc cref="IPlatformService.RunShellAsync(string, bool)"/>
    public static ValueTask RunShellAsync(string script, bool admin)
        => RunShellAsync(script, admin, 3);

    /// <inheritdoc cref="IPlatformService.RunShellAsync(string, bool)"/>
    static async ValueTask RunShellAsync(string script, bool admin, sbyte retry_count)
    {
        if (retry_count <= 0) return;
        var scriptContent = new StringBuilder();
        if (admin)
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
            Toast.Show(msg);
    }

    /// <summary>
    /// 执行脚本
    /// </summary>
    /// <param name="shell">脚本指令</param>
    /// <returns>返回结果</returns>
    public static string RunShell(string shell)
        => Process2.RunShell(Process2.BinBash, shell, e => e.LogAndShowT(TAG));
}