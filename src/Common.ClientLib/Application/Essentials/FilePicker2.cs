using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static System.Application.Services.IFilePickerPlatformService;
using _ThisAssembly = System.Properties.ThisAssembly;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    /// <summary>
    /// 文件选取器，参考 Xamarin.Essentials.FilePicker。
    /// <para><see cref="https://docs.microsoft.com/zh-cn/xamarin/essentials/file-picker"/></para>
    /// <para><see cref="https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/FilePicker/FilePicker.shared.cs"/></para>
    /// </summary>
    public static partial class FilePicker2
    {
        static readonly Lazy<bool> mIsSupportedSaveFileDialog = new(() =>
        {
            var s = ISaveFileDialogService.Instance;
            return s != null;
        });

        /// <summary>
        /// 是否支持保存文件对话框。
        /// </summary>
        public static bool IsSupportedSaveFileDialog => mIsSupportedSaveFileDialog.Value;

        /// <summary>
        /// 启动文件选择器以选择单个文件。
        /// </summary>
        /// <param name="options">要使用的文件选择器选项，可能为空。</param>
        /// <returns>文件拾取结果对象，或当用户取消拾取时为空。</returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static async Task<FileResult?> PickAsync(PickOptions? options = null)
        {
            try
            {
                if (Essentials.IsSupported)
                {
                    return await FilePicker.PickAsync(options);
                }
                else
                {
                    var s = IOpenFileDialogService.Instance;
                    if (s != null)
                    {
                        return (await s.PlatformPickAsync(options))?.FirstOrDefault();
                    }
                    throw new PlatformNotSupportedException();
                }
            }
            catch (PermissionException e)
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return null;
            }
            catch (OperationCanceledException)
            {
                // The user canceled or something went wrong
                return null;
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
            catch (PermissionException e)
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
            }
            catch (OperationCanceledException)
            {
                // The user canceled or something went wrong
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
            catch (PermissionException e)
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
            }
            catch (OperationCanceledException)
            {
                // The user canceled or something went wrong
            }
        }

        /// <summary>
        /// 启动文件选择器以选择多个文件。
        /// </summary>
        /// <param name="options">要使用的文件选择器选项，可能为空。</param>
        /// <returns>文件拾取结果对象，或当用户取消拾取时为空。</returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static async Task<IEnumerable<FileResult>?> PickMultipleAsync(PickOptions? options = null)
        {
            try
            {
                if (Essentials.IsSupported)
                {
                    return await FilePicker.PickMultipleAsync(options);
                }
                else
                {
                    var s = IOpenFileDialogService.Instance;
                    if (s != null)
                    {
                        return await s.PlatformPickAsync(options ?? PickOptions.Default, true);
                    }
                    throw new PlatformNotSupportedException();
                }
            }
            catch (PermissionException e)
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return null;
            }
            catch (OperationCanceledException)
            {
                // The user canceled or something went wrong
                return null;
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
            catch (PermissionException e)
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
            }
            catch (OperationCanceledException)
            {
                // The user canceled or something went wrong
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
            catch (PermissionException e)
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
            }
            catch (OperationCanceledException)
            {
                // The user canceled or something went wrong
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
            catch (PermissionException e)
            {
                Toast.Show(e.Message); // Xamarin.Essentials.PermissionException
                return null;
            }
            catch (OperationCanceledException)
            {
                // The user canceled or something went wrong
                return null;
            }
        }

        public sealed class SaveFileResult : IDisposable
        {
            readonly string? fullPath;
            readonly Stream? stream;
            readonly string? @string;
            bool disposedValue;

            public SaveFileResult(string fullPath)
            {
                this.fullPath = fullPath;
            }

            public SaveFileResult(Stream stream, string? @string = null)
            {
                this.stream = stream;
                this.@string = @string;
            }

            /// <summary>
            /// 打开写入流，在桌面平台上为 <see cref="FileStream"/>，在 Android 上为 Java.IO.OutputStream
            /// </summary>
            /// <returns></returns>
            /// <exception cref="NotSupportedException"></exception>
            public Stream OpenWrite()
            {
                if (fullPath != null)
                    return new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
                if (stream != null)
                    return stream;
                throw new NotSupportedException();
            }

            public override string ToString()
            {
                if (fullPath != null)
                    return fullPath;
                if (@string != null)
                    return @string;
                return base.ToString();
            }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 释放托管状态(托管对象)
                        stream?.Dispose();
                    }

                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                    // TODO: 将大型字段设置为 null
                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}