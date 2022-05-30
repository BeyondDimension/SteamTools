namespace System.Application.Services.Implementation;

public class PermissionsPlatformServiceImpl : IPermissionsPlatformService
{
    /// <summary>
    /// 权限被拒绝，可提示用户在“设置”中启用，通常应有不再提示的选项
    /// </summary>
    /// <param name="permission"></param>
    protected virtual void PromptUserGoToSettings(Permissions.BasePermission permission)
    {
        // 可由平台重写此函数实现，通常为显示一个UI上的弹窗，上有复选框不再显示
    }

    /// <summary>
    /// 提示用户需要权限的其他信息
    /// </summary>
    /// <param name="permission"></param>
    protected virtual void ShowRationale(Permissions.BasePermission permission)
    {
        // 可由平台重写此函数实现，通常为显示一个吐司消息，因调用此函数后会立即开始重新申请权限
    }

    public async Task<PermissionStatus> CheckAndRequestAsync(Permissions.BasePermission permission)
    {
        // https://docs.microsoft.com/zh-cn/xamarin/essentials/permissions?context=xamarin%2Fandroid&tabs=android#general-usage
        var status = (await permission.CheckStatusAsync()).Convert();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
        {
            // Prompt the user to turn on in settings
            // 提示用户在“设置”中启用
            // On iOS once a permission has been denied it may not be requested again from the application
            // 在iOS上，一旦权限被拒绝，就不能再次从应用程序请求权限
            PromptUserGoToSettings(permission);
            return status;
        }

        if (permission.ShouldShowRationale())
        {
            // Prompt the user with additional information as to why the permission is needed
            // 提示用户需要权限的其他信息
            ShowRationale(permission);
        }

        status = (await permission.RequestAsync()).Convert();

        if (status == PermissionStatus.Denied)
        {
            PromptUserGoToSettings(permission);
        }

        return status;
    }

    Task<PermissionStatus> IPermissionsPlatformService.CheckAndRequestAsync(object permission)
        => CheckAndRequestAsync((Permissions.BasePermission)permission);
}
