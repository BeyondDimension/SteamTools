using BaseService = BD.Common.Services.IFilePickerPlatformService;
using IOpenFileDialogService = BD.Common.Services.IFilePickerPlatformService.IOpenFileDialogService;
using ISaveFileDialogService = BD.Common.Services.IFilePickerPlatformService.ISaveFileDialogService;
using IServiceBase = BD.Common.Services.IFilePickerPlatformService.IServiceBase;
using OpenFileDialog = Avalonia.Controls.OpenFileDialog;
using SaveFileDialog = Avalonia.Controls.SaveFileDialog;

namespace BD.WTTS.Services.Implementation;

sealed class AvaloniaFilePickerPlatformService : BaseService, IServiceBase, IOpenFileDialogService, ISaveFileDialogService
{
    IOpenFileDialogService BaseService.OpenFileDialogService => this;

    ISaveFileDialogService BaseService.SaveFileDialogService => this;

    static Window MainWindow
    {
        get
        {
            var mainWindow = App.Instance.MainWindow;
            return mainWindow.ThrowIsNull();
        }
    }

    // https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/FilePicker/FilePicker.uwp.cs
    // https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/FileSystem/FileSystem.shared.cs

    IFilePickerFileType IInternalFilePickerPlatformService.Images => FilePickerFileType.Parse(FileEx.Images);

    IFilePickerFileType IInternalFilePickerPlatformService.Png => FilePickerFileType.Parse(new[] {
        FileEx.PNG,
    });

    IFilePickerFileType IInternalFilePickerPlatformService.Jpeg => FilePickerFileType.Parse(new[] {
        FileEx.JPG,
        FileEx.JPEG,
    });

    IFilePickerFileType IInternalFilePickerPlatformService.Videos => FilePickerFileType.Parse(new[] {
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
    });

    IFilePickerFileType IInternalFilePickerPlatformService.Pdf => FilePickerFileType.Parse(new[] {
        FileEx.PDF,
    });

    static List<FileDialogFilter>? Convert(IFilePickerFileType? fileTypes)
    {
        if (fileTypes is FilePickerFileType.IFilePickerFileTypeWithName @interface)
        {
            var values = @interface.GetFileTypes();
            if (values.Any())
            {
                return values.Select(x => new FileDialogFilter
                {
                    Name = x.Item1,
                    Extensions = IServiceBase.FormatExtensions(x.Item2, trimLeadingPeriod: true).ToList(),
                }).ToList();
            }
        }
        else
        {
            var extensions = fileTypes?.GetPlatformFileType(DeviceInfo2.Platform());
            if (extensions.Any_Nullable())
            {
                return new()
                    {
                        new FileDialogFilter
                        {
                            Extensions = IServiceBase.FormatExtensions(extensions, trimLeadingPeriod: true).ToList(),
                        },
                    };
            }
        }
        return null;
    }

    async Task<IEnumerable<IFileResult>> IOpenFileDialogService.PlatformPickAsync(PickOptions? options, bool allowMultiple)
    {
        OpenFileDialog fileDialog = new()
        {
            AllowMultiple = allowMultiple,
        };
        if (options != default)
        {
            if (options.PickerTitle != default)
            {
                fileDialog.Title = options.PickerTitle;
            }
            if (options.FileTypes != default)
            {
                var filters = Convert(options.FileTypes);
                if (filters != default)
                {
                    fileDialog.Filters = filters;
                }
            }
        }

        var fileResults = await fileDialog.ShowAsync(MainWindow);

        return fileResults.Any_Nullable() ? fileResults.Select(x => new FileResult(x)) : Array.Empty<FileResult>();
    }

    async Task<SaveFileResult?> ISaveFileDialogService.PlatformSaveAsync(PickOptions? options)
    {
        SaveFileDialog fileDialog = new();
        if (options != default)
        {
            if (options.PickerTitle != default)
            {
                fileDialog.Title = options.PickerTitle;
            }
            if (options.FileTypes != default)
            {
                var filters = Convert(options.FileTypes);
                if (filters != default)
                {
                    fileDialog.Filters = filters;
                }
            }
            if (options.InitialFileName != default)
            {
                fileDialog.InitialFileName = options.InitialFileName;
            }
        }

        var fileResult = await fileDialog.ShowAsync(MainWindow);

        return string.IsNullOrEmpty(fileResult) ? null : new(fileResult);
    }
}
