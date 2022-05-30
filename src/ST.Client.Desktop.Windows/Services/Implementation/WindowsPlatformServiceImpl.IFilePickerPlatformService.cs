using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using static System.Application.Services.IFilePickerPlatformService;
using static System.Application.Services.IFilePickerPlatformService.IServiceBase;

namespace System.Application.Services.Implementation
{
    partial class WindowsPlatformServiceImpl : IOpenFileDialogService
    {
        // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/FilePicker/FilePicker.uwp.cs

        [SupportedOSPlatform("Windows10.0.10240.0")]
        public async Task<IEnumerable<IFileResult>> PlatformPickAsync(PickOptions? options, bool allowMultiple = false)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            SetFileTypes(options, picker);

            var resultList = new List<StorageFile>();

            if (allowMultiple)
            {
                var fileList = await picker.PickMultipleFilesAsync();
                if (fileList != null)
                    resultList.AddRange(fileList);
            }
            else
            {
                var file = await picker.PickSingleFileAsync();
                if (file != null)
                    resultList.Add(file);
            }

            foreach (var file in resultList)
                StorageApplicationPermissions.FutureAccessList.Add(file);

            return resultList.Select(storageFile => new FileResult(storageFile.Path));
        }

        [SupportedOSPlatform("Windows10.0.10240.0")]
        static void SetFileTypes(PickOptions? options, FileOpenPicker picker)
        {
            var values = FormatExtensions(options?.FileTypes?.GetPlatformFileType(DeviceInfo2.Platform()));
            foreach (var item in values)
            {
                picker.FileTypeFilter.Add(item);
            }
        }
    }
}