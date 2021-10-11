using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static System.Application.Services.IFilePickerPlatformService;
using _ThisAssembly = System.Properties.ThisAssembly;

namespace System.Application
{
    /// <summary>
    /// 文件选取器，参考 Xamarin.Essentials.FilePicker
    /// <para><see cref="https://docs.microsoft.com/zh-cn/xamarin/essentials/file-picker"/></para>
    /// <para><see cref="https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/FilePicker/FilePicker.shared.cs"/></para>
    /// </summary>
    public static class FilePicker2
    {
        static readonly Lazy<bool> mIsSupportedFileExtensionFilter = new(() =>
        {
            if (XamarinEssentials.IsSupported)
            {
                return OperatingSystem2.IsWindows;
            }
            else
            {
                var s = IOpenFileDialogService.Instance;
                if (s != null)
                {
                    return s.IsSupportedFileExtensionFilter;
                }
                return false;
            }
        });
        /// <summary>
        /// 是否支持文件扩展名过滤
        /// </summary>
        public static bool IsSupportedFileExtensionFilter => mIsSupportedFileExtensionFilter.Value;

        public static async Task<FileResult?> PickAsync(PickOptions? options = null)
        {
            if (XamarinEssentials.IsSupported)
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
                        Toast.Show(e.Message);
                    }
                }
            }
            catch (PermissionException e)
            {
                Toast.Show(e.Message);
            }
            catch
            {
                // The user canceled or something went wrong
            }
        }

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
                        Toast.Show(e.Message);
                    }
                }
            }
            catch (PermissionException e)
            {
                Toast.Show(e.Message);
            }
            catch
            {
                // The user canceled or something went wrong
            }
        }

        public static async Task<IEnumerable<FileResult>> PickMultipleAsync(PickOptions? options = null)
        {
            if (XamarinEssentials.IsSupported)
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

        public static async Task PickMultipleAsync(Action<IEnumerable<string>> action, FilePickerFileType? fileTypes = null, string? title = _ThisAssembly.AssemblyTrademark)
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
                        Toast.Show(e.Message);
                    }
                }
            }
            catch (PermissionException e)
            {
                Toast.Show(e.Message);
            }
            catch
            {
                // The user canceled or something went wrong
            }
        }

        public static async Task PickMultipleAsync(Func<IEnumerable<string>, Task> func, FilePickerFileType? fileTypes = null, string? title = _ThisAssembly.AssemblyTrademark)
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
                        Toast.Show(e.Message);
                    }
                }
            }
            catch (PermissionException e)
            {
                Toast.Show(e.Message);
            }
            catch
            {
                // The user canceled or something went wrong
            }
        }

        public static async Task<FileResult?> SaveAsync(SaveOptions? options = null)
        {
            var s = ISaveFileDialogService.Instance;
            if (s != null)
            {
                return await s.PlatformSaveAsync(options);
            }
            throw new PlatformNotSupportedException();
        }

        public class SaveOptions : PickOptions
        {
            public string? InitialFileName { get; set; }
        }

        public class GeneralFilePickerFileType : FilePickerFileType
        {
            readonly IEnumerable<string> values;

            public GeneralFilePickerFileType(IEnumerable<string> values) : base()
            {
                this.values = values;
            }

            public static implicit operator GeneralFilePickerFileType(string[] values) => new(values);

            public static implicit operator GeneralFilePickerFileType(List<string> values) => new(values);

            protected override IEnumerable<string> GetPlatformFileType(DevicePlatform _) => values;
        }

        public class FilePickerFileType2 : FilePickerFileType
        {
            readonly IReadOnlyDictionary<Platform, IEnumerable<string>> fileTypes;

            public FilePickerFileType2(IReadOnlyDictionary<Platform, IEnumerable<string>> fileTypes) : base() => this.fileTypes = fileTypes;

            public static implicit operator FilePickerFileType2(Dictionary<Platform, IEnumerable<string>> fileTypes) => new(fileTypes);

            protected override IEnumerable<string> GetPlatformFileType(DevicePlatform _)
            {
                if (fileTypes.TryGetValue(DeviceInfo2.Platform, out var type))
                    return type;

                throw new PlatformNotSupportedException("This platform does not support this file type.");
            }
        }

        public class FilePickerFilter : GeneralFilePickerFileType
        {
            public IEnumerable<(string name, IEnumerable<string> extensions)> Filters { get; }

            public FilePickerFilter(IEnumerable<(string name, IEnumerable<string> extensions)> filters) : base(filters.SelectMany(x => x.extensions))
            {
                Filters = filters;
            }
        }

        public static class Images
        {
            static readonly Lazy<FilePickerFileType> _FileTypes = new(() =>
            {
                if (XamarinEssentials.IsSupported)
                {
                    return FilePickerFileType.Images;
                }
                else if (OperatingSystem2.IsWindows)
                {
                    GeneralFilePickerFileType fileTypes = new[] { FileEx.PNG, FileEx.JPG, FileEx.JPEG, FileEx.GIF, FileEx.BMP };
                    return fileTypes;
                }
                else
                {
                    GeneralFilePickerFileType fileTypes = new[] { FileEx.PNG, FileEx.JPG, FileEx.JPEG, FileEx.GIF };
                    return fileTypes;
                }
            });

            /// <inheritdoc cref="FilePickerFileType.Images"/>
            public static FilePickerFileType FileTypes => _FileTypes.Value;

            /// <inheritdoc cref="PickOptions.Images"/>
            public static PickOptions PickOptions => new()
            {
                FileTypes = FileTypes,
            };
        }
    }
}