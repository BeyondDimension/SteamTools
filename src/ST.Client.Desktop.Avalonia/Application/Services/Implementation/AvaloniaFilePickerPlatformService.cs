using Avalonia.Controls;
using System.Application.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static System.Application.Services.IFilePickerPlatformService;
using static System.Application.Services.IFilePickerPlatformService.IServiceBase;

namespace System.Application.Services.Implementation
{
    internal sealed class AvaloniaFilePickerPlatformService : IFilePickerPlatformService, IOpenFileDialogService, ISaveFileDialogService
    {
        IOpenFileDialogService IFilePickerPlatformService.OpenFileDialogService => this;

        ISaveFileDialogService IFilePickerPlatformService.SaveFileDialogService => this;

        static List<FileDialogFilter>? Convert(FilePickerFileType fileTypes)
        {
            if (fileTypes is FilePicker2.FilePickerFilter filters)
            {
                if (filters.Filters.Any())
                {
                    return filters.Filters.Select(x => new FileDialogFilter
                    {
                        Name = x.name,
                        Extensions = FormatExtensions(x.extensions, trimLeadingPeriod: true),
                    }).ToList();
                }
            }
            else
            {
                var extensions = fileTypes.Value;
                if (extensions.Any())
                {
                    return new()
                    {
                        new FileDialogFilter
                        {
                            Extensions = FormatExtensions(extensions, trimLeadingPeriod: true),
                        },
                    };
                }
            }
            return null;
        }

        async Task<IEnumerable<FileResult>> IOpenFileDialogService.PlatformPickAsync(PickOptions? options, bool allowMultiple)
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

            var fileResults = await fileDialog.ShowAsync(IAvaloniaApplication.Instance.MainWindow);

            return fileResults.Any_Nullable() ? fileResults.Select(x => new FileResult(x)) : Array.Empty<FileResult>();
        }

        async Task<FileResult?> ISaveFileDialogService.PlatformSaveAsync(FilePicker2.SaveOptions? options)
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

            var fileResult = await fileDialog.ShowAsync(IAvaloniaApplication.Instance.MainWindow);

            return string.IsNullOrEmpty(fileResult) ? null : new FileResult(fileResult);
        }

        bool IOpenFileDialogService.IsSupportedFileExtensionFilter => true;
    }
}