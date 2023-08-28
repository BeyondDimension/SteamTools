#if IOS || MACOS || MACCATALYST
using MobileCoreServices;
#endif
using AvaloniaFilePickerFileType = Avalonia.Platform.Storage.FilePickerFileType;
using BaseService = BD.Common.Services.IFilePickerPlatformService;
using FilePickerFileType = BD.Common.Models.FilePickerFileType;
using IOpenFileDialogService = BD.Common.Services.IFilePickerPlatformService.IOpenFileDialogService;
using ISaveFileDialogService = BD.Common.Services.IFilePickerPlatformService.ISaveFileDialogService;
using IServiceBase = BD.Common.Services.IFilePickerPlatformService.IServiceBase;

namespace BD.WTTS.Services.Implementation;

sealed class AvaloniaFilePickerPlatformService : BaseService, IServiceBase, IOpenFileDialogService, ISaveFileDialogService
{
    IOpenFileDialogService BaseService.OpenFileDialogService => this;

    ISaveFileDialogService BaseService.SaveFileDialogService => this;

    // https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/FilePicker/FilePicker.uwp.cs
    // https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/FileSystem/FileSystem.shared.cs

    IFilePickerFileType IInternalFilePickerPlatformService.Images { get; } = FilePickerFileType.Parse(new string[] {
           "*.webp", "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "public.image", "image/*",
        });

    IFilePickerFileType IInternalFilePickerPlatformService.Png { get; } = FilePickerFileType.Parse(new string[] {
           "*.png", "public.png", "image/png",
        });

    IFilePickerFileType IInternalFilePickerPlatformService.Jpeg { get; } = FilePickerFileType.Parse(new string[] {
           "*.jpg", "*.jpeg",  "public.jpeg", "image/jpeg",
        });

    IFilePickerFileType IInternalFilePickerPlatformService.Videos => throw new NotImplementedException();

    IFilePickerFileType IInternalFilePickerPlatformService.Pdf { get; } = FilePickerFileType.Parse(new string[] {
           "*.pdf", "application/pdf", "com.adobe.pdf",
        });

    //#if IOS || MACOS || MACCATALYST
    //    static readonly Lazy<string?[]> _UTTypes = new(() =>
    //    {
    //        var fields = typeof(UTType).GetFields(BindingFlags.Public | BindingFlags.Static);
    //        if (fields == null)
    //            return Array.Empty<string>();
    //        var result = fields.Where(x => x.FieldType == typeof(string) || x.FieldType == typeof(NSString)).Select(x => x.GetValue(null)?.ToString()).ToArray();
    //        return result;
    //    });
    //#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsAppleUniformTypeIdentifier(string s)
    {
        if (s == "jpeg" || s.StartsWith("public.") || s.StartsWith("com."))
        {
            return true;
        }
        //#if IOS || MACOS || MACCATALYST
        //        if (_UTTypes.Value.Contains(s))
        //        {
        //            return true;
        //        }
        //#endif
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static AvaloniaFilePickerFileType Convert(string name, IEnumerable<string>? extensions)
    {
        // https://docs.avaloniaui.net/docs/next/concepts/services/storage-provider/file-picker-options
        var result = new AvaloniaFilePickerFileType(name);
        if (extensions != null)
        {
#if IOS || MACOS || MACCATALYST
            HashSet<string> appleUniformTypeIdentifiers = new();
#else
#if !WINDOWS
            HashSet<string> mimeTypes = new();
#endif
            HashSet<string> patterns = new();
#endif
            foreach (var x in extensions)
            {
                if (x.Contains('/'))
                {
#if !WINDOWS && !(IOS || MACOS || MACCATALYST)
                    mimeTypes.Add(x);
#endif
                    continue;
                }
                else if (IsAppleUniformTypeIdentifier(x))
                {
#if IOS || MACOS || MACCATALYST
                    appleUniformTypeIdentifiers.Add(x);
#endif
                    continue;
                }
                else if (x.StartsWith('.'))
                {
#if !(IOS || MACOS || MACCATALYST)
                    patterns.Add($"*{x}");
#endif
                    continue;
                }
                else if (x.StartsWith('*'))
                {
#if !(IOS || MACOS || MACCATALYST)
                    patterns.Add(x);
#endif
                    continue;
                }
                else
                {
#if !(IOS || MACOS || MACCATALYST)
                    patterns.Add($"*.{x}");
#endif
                    continue;
                }
            }
#if IOS || MACOS || MACCATALYST
            if (appleUniformTypeIdentifiers.Any())
            {
                result.AppleUniformTypeIdentifiers = appleUniformTypeIdentifiers.ToArray();
            }
#else
#if !WINDOWS
            if (mimeTypes.Any())
            {
                result.MimeTypes = mimeTypes.ToArray();
            }
#endif
            if (patterns.Any())
            {
                result.Patterns = patterns.ToArray();
            }
#endif
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static List<AvaloniaFilePickerFileType>? Convert(IFilePickerFileType? fileTypes)
    {
        if (fileTypes is FilePickerFileType.IFilePickerFileTypeWithName @interface)
        {
            var values = @interface.GetFileTypes();
            if (values.Any())
            {
                return values.Select(x => Convert(x.Item1, x.Item2)).ToList();
            }
        }
        else
        {
            var extensions = fileTypes?.GetPlatformFileType(DeviceInfo2.Platform());
            if (extensions.Any_Nullable())
            {
                return new()
                {
                    Convert(string.Empty, extensions),
                };
            }
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IEnumerable<IFileResult> Convert(IReadOnlyList<IStorageFile> fileResults)
    {
        foreach (var fileResult in fileResults)
        {
            var filePath = GetAbsoluteFilePath(fileResult);
            if (!string.IsNullOrEmpty(filePath))
                yield return new FileResult(filePath);
        }
    }

    async Task<IEnumerable<IFileResult>> IOpenFileDialogService.PlatformPickAsync(PickOptions? options, bool allowMultiple)
    {
        var storageProvider = App.Instance.MainWindow?.StorageProvider;
        if (storageProvider == null || !storageProvider.CanOpen)
            return Array.Empty<IFileResult>();

        FilePickerOpenOptions options_ = new()
        {
            AllowMultiple = allowMultiple,
        };
        if (options != default)
        {
            if (options.PickerTitle != default)
            {
                options_.Title = options.PickerTitle;
            }
            if (options.FileTypes != default)
            {
                var filters = Convert(options.FileTypes);
                if (filters != default)
                {
                    options_.FileTypeFilter = filters;
                }
            }
        }

        var fileResults = await storageProvider.OpenFilePickerAsync(options_);

        if (fileResults.Any_Nullable())
        {
            return Convert(fileResults);
        }
        return Array.Empty<FileResult>();
    }

    async Task<SaveFileResult?> ISaveFileDialogService.PlatformSaveAsync(PickOptions? options)
    {
        var storageProvider = App.Instance.MainWindow?.StorageProvider;
        if (storageProvider == null || !storageProvider.CanSave)
            return null;

        FilePickerSaveOptions options_ = new();

        if (options != default)
        {
            if (options.PickerTitle != default)
            {
                options_.Title = options.PickerTitle;
            }
            if (options.FileTypes != default)
            {
                var filters = Convert(options.FileTypes);
                if (filters != default)
                {
                    options_.FileTypeChoices = filters;
                }
            }
            if (options.InitialFileName != default)
            {
                options_.SuggestedFileName = options.InitialFileName;
            }
        }

        var fileResult = await storageProvider.SaveFilePickerAsync(options_);
        var filePath = GetAbsoluteFilePath(fileResult);
        return string.IsNullOrEmpty(filePath) ? null : new(filePath);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string? GetAbsoluteFilePath(IStorageItem? storageItem)
    {
        const string trim_s =
#if WINDOWS
            "file:///";
#else
            "file://";
#endif
        var value = storageItem?.Path?.ToString()?.TrimStart(trim_s);
        return value;
    }
}
