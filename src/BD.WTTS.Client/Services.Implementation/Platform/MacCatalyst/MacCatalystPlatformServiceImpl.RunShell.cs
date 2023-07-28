#if MACOS || MACCATALYST || IOS
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    static async ValueTask RunShellCoreAsync(string script, bool requiredAdministrator)
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

    public ValueTask RunShellAsync(string script, bool requiredAdministrator)
        => RunShellCoreAsync(script, requiredAdministrator);
}
#endif