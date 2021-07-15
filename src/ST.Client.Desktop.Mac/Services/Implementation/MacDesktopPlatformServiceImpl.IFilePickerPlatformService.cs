//using MonoMac.AppKit;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Xamarin.Essentials;
//using static System.Application.Services.IFilePickerPlatformService;
//using static System.Application.Services.IFilePickerPlatformService.IServiceBase;

//namespace System.Application.Services.Implementation
//{
//    partial class MacDesktopPlatformServiceImpl : IOpenFileDialogService
//    {
//        // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/FilePicker/FilePicker.macos.cs

//        public Task<IEnumerable<FileResult>> PlatformPickAsync(PickOptions? options, bool allowMultiple = false)
//        {
//            var openPanel = new NSOpenPanel
//            {
//                CanChooseFiles = true,
//                AllowsMultipleSelection = allowMultiple,
//                CanChooseDirectories = false
//            };

//            if (options?.PickerTitle != null)
//                openPanel.Title = options.PickerTitle;

//            SetFileTypes(options, openPanel);

//            var resultList = new List<FileResult>();
//            var panelResult = openPanel.RunModal();
//            if (panelResult == (nint)(long)NSModalResponse.OK)
//            {
//                foreach (var url in openPanel.Urls)
//                    resultList.Add(new FileResult(url.Path));
//            }

//            return Task.FromResult<IEnumerable<FileResult>>(resultList);
//        }

//        static void SetFileTypes(PickOptions? options, NSOpenPanel panel)
//        {
//            var allowedFileTypes = new List<string>();

//            if (options?.FileTypes?.Value != null)
//            {
//                foreach (var type in options.FileTypes.Value)
//                {
//                    allowedFileTypes.Add(type.TrimStart('*', '.'));
//                }
//            }

//            panel.AllowedFileTypes = allowedFileTypes.ToArray();
//        }
//    }
//}
