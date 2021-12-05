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
    static readonly Lazy<string> _bin_bash = new(() => string.Format("{0}bin{0}bash", Path.DirectorySeparatorChar));

    /// <summary>
    /// /bin/bash
    /// </summary>
    public static string BinBash => _bin_bash.Value;

    /// <inheritdoc cref="IPlatformService.RunShellAsync(string, bool)"/>
    public static ValueTask RunShellAsync(string script, bool admin)
        => RunShellAsync(script, admin, 3);

    /// <inheritdoc cref="IPlatformService.RunShellAsync(string, bool)"/>
    static async ValueTask RunShellAsync(string script, bool admin, sbyte retry_count)
    {
        if (retry_count <= 0) return;

        var file = new FileInfo(Path.Combine(IOPath.AppDataDirectory, $@"{(admin ? "sudo" : "")}shell.sh"));

        if (file.Exists)
            file.Delete();
        var scriptContent = new StringBuilder();
        if (admin)
        {
            TextBoxWindowViewModel vm = new()
            {
                Title = AppResources.MacSudoPasswordTips,
                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
                Description = $"sudo {script}",
            };
            await TextBoxWindowViewModel.ShowDialogAsync(vm);
            if (string.IsNullOrWhiteSpace(vm.Value))
                return;
            scriptContent.AppendLine($"echo \"{vm.Value}\" | sudo -S {script}");
        }
        else
        {
            scriptContent.AppendLine(script);
        }
        using (var stream = file.CreateText())
        {
            stream.Write(scriptContent);
            stream.Flush();
        }
        using var p = new Process();
        p.StartInfo.FileName = BinBash;
        p.StartInfo.Arguments = $"\"{file.FullName}\"";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Exited += async (_, _) =>
        {
            if (file.Exists)
                file.Delete();
            if (p.ExitCode != 0)
            {
                await RunShellAsync(script, admin, --retry_count);
            }
        };
        p.Start();
        var ret = p.StandardOutput.ReadToEnd();
        p.Kill();
        if (file.Exists)
            file.Delete();
    }
}