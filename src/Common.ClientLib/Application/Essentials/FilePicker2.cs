using static System.Application.Services.IFilePickerPlatformService;
using _ThisAssembly = System.Properties.ThisAssembly;

// ReSharper disable once CheckNamespace
namespace System.Application;

/// <summary>
/// 文件选取器，参考 Essentials.FilePicker。
/// <para><see cref="https://docs.microsoft.com/zh-cn/xamarin/essentials/file-picker"/></para>
/// <para><see cref="https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/FilePicker/FilePicker.shared.cs"/></para>
/// </summary>
public static partial class FilePicker2
{
    /// <summary>
    /// 启动文件选择器以选择单个文件。
    /// </summary>
    /// <param name="options">要使用的文件选择器选项，可能为空。</param>
    /// <returns>文件拾取结果对象，或当用户取消拾取时为空。</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static async Task<IFileResult?> PickAsync(PickOptions? options = null)
    {
        try
        {
            var s = IOpenFileDialogService.Instance;
            if (s != null)
            {
                return (await s.PlatformPickAsync(options))?.FirstOrDefault();
            }
            throw new PlatformNotSupportedException();
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                // The user canceled or something went wrong
                return null;
            }
            else if (e.GetType().Name == "PermissionException")
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return null;
            }
            else
            {
                throw;
            }
        }
    }

    const string TAG = nameof(FilePicker2);

    /// <summary>
    /// 启动文件选择器以选择单个文件。
    /// </summary>
    /// <param name="action"></param>
    /// <param name="fileTypes"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static async Task PickAsync(Action<string> action, FilePickerFileType? fileTypes = null, string? title = _ThisAssembly.AssemblyTrademark)
    {
        try
        {
            PickOptions o = new();
            if (!string.IsNullOrEmpty(title))
            {
                o.PickerTitle = title;
            }
            if (fileTypes != null)
            {
                o.FileTypes = fileTypes;
            }
            var result = await PickAsync(o);
            if (!string.IsNullOrEmpty(result?.FullPath))
            {
                try
                {
                    action(result.FullPath);
                }
                catch (Exception e)
                {
                    e.LogAndShowT(TAG);
                }
            }
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                // The user canceled or something went wrong
                return;
            }
            else if (e.GetType().Name == "PermissionException")
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return;
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 启动文件选择器以选择单个文件。
    /// </summary>
    /// <param name="func"></param>
    /// <param name="fileTypes"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static async Task PickAsync(Func<string, Task> func, FilePickerFileType? fileTypes = null, string? title = _ThisAssembly.AssemblyTrademark)
    {
        try
        {
            PickOptions o = new();
            if (!string.IsNullOrEmpty(title))
            {
                o.PickerTitle = title;
            }
            if (fileTypes != null)
            {
                o.FileTypes = fileTypes;
            }
            var result = await PickAsync(o);
            if (!string.IsNullOrEmpty(result?.FullPath))
            {
                try
                {
                    await func(result.FullPath);
                }
                catch (Exception e)
                {
                    e.LogAndShowT(TAG);
                }
            }
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                // The user canceled or something went wrong
                return;
            }
            else if (e.GetType().Name == "PermissionException")
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return;
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 启动文件选择器以选择多个文件。
    /// </summary>
    /// <param name="options">要使用的文件选择器选项，可能为空。</param>
    /// <returns>文件拾取结果对象，或当用户取消拾取时为空。</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static async Task<IEnumerable<IFileResult>?> PickMultipleAsync(PickOptions? options = null)
    {
        try
        {
            var s = IOpenFileDialogService.Instance;
            if (s != null)
            {
                return await s.PlatformPickAsync(options ?? PickOptions.Default, true);
            }
            throw new PlatformNotSupportedException();
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                // The user canceled or something went wrong
                return null;
            }
            else if (e.GetType().Name == "PermissionException")
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return null;
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 启动文件选择器以选择多个文件。
    /// </summary>
    /// <param name="action"></param>
    /// <param name="fileTypes"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static async Task PickMultipleAsync(Action<IEnumerable<string>?> action, FilePickerFileType? fileTypes = null, string? title = _ThisAssembly.AssemblyTrademark)
    {
        try
        {
            PickOptions o = new();
            if (!string.IsNullOrEmpty(title))
            {
                o.PickerTitle = title;
            }
            if (fileTypes != null)
            {
                o.FileTypes = fileTypes;
            }
            var result = await PickMultipleAsync(o);
            if (result != null && result.Any())
            {
                try
                {
                    action(result.Select(x => x.FullPath));
                }
                catch (Exception e)
                {
                    e.LogAndShowT(TAG);
                }
            }
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                // The user canceled or something went wrong
                return;
            }
            else if (e.GetType().Name == "PermissionException")
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return;
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 启动文件选择器以选择多个文件。
    /// </summary>
    /// <param name="func"></param>
    /// <param name="fileTypes"></param>
    /// <param name="title"></param>
    /// <returns></returns>
    public static async Task PickMultipleAsync(Func<IEnumerable<string>?, Task> func, FilePickerFileType? fileTypes = null, string? title = _ThisAssembly.AssemblyTrademark)
    {
        try
        {
            PickOptions o = new();
            if (!string.IsNullOrEmpty(title))
            {
                o.PickerTitle = title;
            }
            if (fileTypes != null)
            {
                o.FileTypes = fileTypes;
            }
            var result = await PickMultipleAsync(o);
            if (result != null && result.Any())
            {
                try
                {
                    await func(result.Select(x => x.FullPath));
                }
                catch (Exception e)
                {
                    e.LogAndShowT(TAG);
                }
            }
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                // The user canceled or something went wrong
                return;
            }
            else if (e.GetType().Name == "PermissionException")
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return;
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// 启动文件保存弹窗以保存单个文件
    /// </summary>
    /// <param name="options">要使用的文件保存弹窗选项，可能为空。</param>
    /// <returns>文件拾取结果对象，或当用户取消拾取时为空。</returns>
    /// <exception cref="PlatformNotSupportedException"></exception>
    public static async Task<SaveFileResult?> SaveAsync(SaveOptions? options = null)
    {
        try
        {
            var s = ISaveFileDialogService.Instance;
            if (s != null)
            {
                return await s.PlatformSaveAsync(options);
            }
            throw new PlatformNotSupportedException();
        }
        catch (Exception e)
        {
            if (e is OperationCanceledException)
            {
                // The user canceled or something went wrong
                return null;
            }
            else if (e.GetType().Name == "PermissionException")
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return null;
            }
            else
            {
                throw;
            }
        }
    }
}