using Android.Content;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using static AndroidX.Activity.Result.ActivityResultTask;
#if NET6_0_OR_GREATER
using XEPlatform = Microsoft.Maui.ApplicationModel.Platform;
#else
using XEPlatform = Xamarin.Essentials.Platform;
#endif

namespace System.Application.Services.Implementation
{
    sealed class FilePickerPlatformServiceImpl : IFilePickerPlatformService.ISaveFileDialogService
    {
        public async Task<SaveFileResult?> PlatformSaveAsync(SaveOptions? options)
        {
            // https://developer.android.google.cn/training/data-storage/shared/documents-files?hl=zh-cn#create-file
            // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/MediaPicker/MediaPicker.android.cs
            var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType(MediaTypeNames.All);
            if (options != null)
            {
                if (!string.IsNullOrEmpty(options.InitialFileName))
                {
                    intent.PutExtra(Intent.ExtraTitle, options.InitialFileName);
                }
                var mimetypes = options.FileTypes?.GetPlatformFileType(DeviceInfo2.Platform());
                if (mimetypes.Any_Nullable())
                {
                    intent.PutExtra(Intent.ExtraMimeTypes, mimetypes.ToArray());
                }
            }
            try
            {
                SaveFileResult? result = null;
                void OnResult(Intent intent)
                {
                    // The uri returned is only temporary and only lives as long as the Activity that requested it,
                    // so this means that it will always be cleaned up by the time we need it because we are using
                    // an intermediate activity.

                    var uri = intent.Data;
                    if (uri != null)
                    {
                        var uriString = uri.ToString();
                        var outputStream = XEPlatform.AppContext.ContentResolver!.OpenOutputStream(uri);
                        result = new SaveFileResult(outputStream ?? throw new ArgumentNullException(nameof(outputStream)), uriString);
                    }
                }

                await IntermediateActivity.StartAsync(intent, requestCodeSaveFileDialog, onResult: OnResult);

                return result;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }
    }
}
