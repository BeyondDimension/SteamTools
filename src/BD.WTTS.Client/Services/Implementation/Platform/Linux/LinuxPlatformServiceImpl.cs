#if LINUX
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed partial class LinuxPlatformServiceImpl : IPlatformService
{
    const string TAG = "LinuxPlatformS";

    public LinuxPlatformServiceImpl()
    {
        // 平台服务依赖关系过于复杂，在构造函数中不得注入任何服务，由函数中延时加载调用服务
    }

    public string? SystemUserPassword { get; set; }

    public async ValueTask<string?> GetSystemUserPasswordIgnoreCacheAsync(sbyte retry)
    {
        TextBoxWindowViewModel vm = null!;
        for (sbyte i = 0; i < retry; i++)
        {
            vm = new()
            {
                Title = AppResources.LinuxSudoTips,
                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
            };
            var pwd = await TextBoxWindowViewModel.ShowDialogAsync(vm);
            if (string.IsNullOrEmpty(pwd) ||
                !string.IsNullOrWhiteSpace(
                    RunShell(nameof(GetSystemUserPasswordIgnoreCacheAsync),
                    $"echo \"{pwd}\" | sudo -S sh -c \"sudo -n true\"")))
            {
                return SystemUserPassword = pwd;
            }
        }
        vm.Title = AppResources.LocalAuth_ProtectionAuth_PasswordError;
        vm.InputType = TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText;
        vm.Show();
        return SystemUserPassword;
    }
}
#endif