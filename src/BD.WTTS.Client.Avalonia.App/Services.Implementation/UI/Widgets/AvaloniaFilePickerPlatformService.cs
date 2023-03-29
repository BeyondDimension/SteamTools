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

    IFilePickerFileType IInternalFilePickerPlatformService.Images =>
#if IOS || MACOS || MACCATALYST
        FilePickerFileType.Parse(new string[] {
            UTType.PNG, UTType.JPEG, "jpeg",
        });
#else
        FilePickerFileType.Parse(FileEx.Images);
#endif

    IFilePickerFileType IInternalFilePickerPlatformService.Png => FilePickerFileType.Parse(new string[] {
#if IOS || MACOS || MACCATALYST
        UTType.PNG,
#else
        FileEx.PNG,
#endif
    });

    IFilePickerFileType IInternalFilePickerPlatformService.Jpeg => FilePickerFileType.Parse(new string[] {
#if IOS || MACOS || MACCATALYST
        UTType.JPEG,
        "jpeg",
#else
        FileEx.JPG,
        FileEx.JPEG,
#endif
    });

    IFilePickerFileType IInternalFilePickerPlatformService.Videos => FilePickerFileType.Parse(new string[] {
#if IOS || MACOS || MACCATALYST
        UTType.MPEG4, UTType.Video, UTType.AVIMovie, UTType.AppleProtectedMPEG4Video, "mp4", "m4v", "mpg", "mpeg", "mp2", "mov", "avi", "mkv", "flv", "gifv", "qt",
#else
        FileEx.Mp4,
        FileEx.Mov,
        FileEx.Avi,
        FileEx.Wmv,
        FileEx.M4v,
        FileEx.Mpg,
        FileEx.Mpeg,
        FileEx.Mp2,
        FileEx.Mkv,
        FileEx.Flv,
        FileEx.Gifv,
        FileEx.Qt,
#endif
    });

    IFilePickerFileType IInternalFilePickerPlatformService.Pdf => FilePickerFileType.Parse(new string[] {
#if IOS || MACOS || MACCATALYST
        UTType.PDF,
#else
        FileEx.PDF,
#endif
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static AvaloniaFilePickerFileType Convert(string name, IEnumerable<string>? extensions)
    {
        var result = new AvaloniaFilePickerFileType(name)
        {
#if IOS || MACOS || MACCATALYST
            AppleUniformTypeIdentifiers = extensions?.ToArray(),
#else
            Patterns = IServiceBase.FormatExtensions(extensions, trimLeadingPeriod: true).ToArray(),
#endif
        };
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
            var filePath = fileResult?.Path?.ToString();
            if (filePath != null)
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
        var filePath = fileResult?.Path?.ToString();
        return string.IsNullOrEmpty(filePath) ? null : new(filePath);
    }
}
