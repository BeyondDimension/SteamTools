using Avalonia.Controls;
using System.Application.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseService = System.Application.Services.IFilePickerPlatformService;
using IOpenFileDialogService = System.Application.Services.IFilePickerPlatformService.IOpenFileDialogService;
using ISaveFileDialogService = System.Application.Services.IFilePickerPlatformService.ISaveFileDialogService;
using IServiceBase = System.Application.Services.IFilePickerPlatformService.IServiceBase;

namespace System.Application.Services.Implementation
{
    internal sealed class AvaloniaFilePickerPlatformService : BaseService, IServiceBase, IOpenFileDialogService, ISaveFileDialogService
    {
        IOpenFileDialogService BaseService.OpenFileDialogService => this;

        ISaveFileDialogService BaseService.SaveFileDialogService => this;

        // https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/FilePicker/FilePicker.uwp.cs
        // https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/FileSystem/FileSystem.shared.cs

        IFilePickerFileType BaseService.Images => FilePickerFileType.Parse(FileEx.ImageFileExtensions);

        IFilePickerFileType BaseService.Png => FilePickerFileType.Parse(new[] {
            FileEx.PNG,
        });

        IFilePickerFileType BaseService.Jpeg => FilePickerFileType.Parse(new[] {
            FileEx.JPG,
            FileEx.JPEG,
        });

        IFilePickerFileType BaseService.Videos => FilePickerFileType.Parse(new[] { FileEx.Mp4, FileEx.Mov, FileEx.Avi, FileEx.Wmv, FileEx.M4v, FileEx.Mpg, FileEx.Mpeg, FileEx.Mp2, FileEx.Mkv, FileEx.Flv, FileEx.Gifv, FileEx.Qt });

        IFilePickerFileType BaseService.Pdf => FilePickerFileType.Parse(new[] {
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

            var fileResults = await fileDialog.ShowAsync(IAvaloniaApplication.Instance.MainWindow!);

            return fileResults.Any_Nullable() ? fileResults.Select(x => new FileResult(x)) : Array.Empty<FileResult>();
        }

        async Task<SaveFileResult?> ISaveFileDialogService.PlatformSaveAsync(SaveOptions? options)
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

            var fileResult = await fileDialog.ShowAsync(IAvaloniaApplication.Instance.MainWindow!);

            return string.IsNullOrEmpty(fileResult) ? null : new(fileResult);
        }
    }
}