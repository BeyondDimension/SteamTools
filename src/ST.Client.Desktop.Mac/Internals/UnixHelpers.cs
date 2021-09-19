using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    static class UnixHelpers
    {
        static readonly Lazy<string> _bin_bash = new(() => string.Format("{0}bin{0}bash", Path.DirectorySeparatorChar));

        /// <summary>
        /// /bin/bash
        /// </summary>
        public static string BinBash => _bin_bash.Value;

        public static ValueTask AdminShellAsync(string shell, bool admin)
            => AdminShellAsync(shell, admin, 3);

        static async ValueTask AdminShellAsync(string shell, bool admin, sbyte retry_count)
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
                    Description = $"sudo {shell}",
                };
                await TextBoxWindowViewModel.ShowDialogAsync(vm);
                if (string.IsNullOrWhiteSpace(vm.Value))
                    return;
                scriptContent.AppendLine($"echo \"{vm.Value}\" | sudo -S {shell}");
            }
            else
            {
                scriptContent.AppendLine(shell);
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
                    await AdminShellAsync(shell, admin, --retry_count);
                }
            };
            p.Start();
            var ret = p.StandardOutput.ReadToEnd();
            p.Kill();
            if (file.Exists)
                file.Delete();
        }
    }
}