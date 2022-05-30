#if MAUI
using E_FilePickerFileType = Microsoft.Maui.Storage.FilePickerFileType;
#else
using E_FilePickerFileType = Xamarin.Essentials.FilePickerFileType;
#endif
using BaseService = System.Application.Services.IFilePickerPlatformService;
using IOpenFileDialogService = System.Application.Services.IFilePickerPlatformService.IOpenFileDialogService;
using ISaveFileDialogService = System.Application.Services.IFilePickerPlatformService.ISaveFileDialogService;

namespace System.Application.Services.Implementation;

sealed class FilePickerPlatformServiceImpl : BaseService, IOpenFileDialogService
{
    IOpenFileDialogService BaseService.OpenFileDialogService => this;

    ISaveFileDialogService BaseService.SaveFileDialogService => throw new NotImplementedException();

    IFilePickerFileType BaseService.Images => E_FilePickerFileType.Images.Convert();

    IFilePickerFileType BaseService.Png => E_FilePickerFileType.Png.Convert();

    IFilePickerFileType BaseService.Jpeg => E_FilePickerFileType.Jpeg.Convert();

    IFilePickerFileType BaseService.Videos => E_FilePickerFileType.Videos.Convert();

    IFilePickerFileType BaseService.Pdf => E_FilePickerFileType.Pdf.Convert();

    async Task<IEnumerable<IFileResult>> IOpenFileDialogService.PlatformPickAsync(PickOptions? options, bool allowMultiple)
    {
        if (allowMultiple)
        {
            return (await FilePicker.PickMultipleAsync(options.Convert())).Convert();
        }
        else
        {
            return new[] { (await FilePicker.PickAsync(options.Convert())).Convert() };
        }
    }
}
