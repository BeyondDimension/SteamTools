using Android.App;
using Android.Content;
using Android.Provider;
using AndroidX.Core.Content;
using System.IO;
using JFile = Java.IO.File;

namespace System.Application
{
    /// <summary>
    /// 页面跳转到平台特定操作系统中的某些页面，例如当前APP设置页
    /// </summary>
    public static class GoToPlatformPages
    {
        /// <summary>
        /// 模拟 Home 键按下效果，回到主页
        /// </summary>
        /// <param name="context"></param>
        public static void MockHomePressed(Context context)
        {
            try
            {
                var intent = new Intent(Intent.ActionMain)
                    .SetFlags(ActivityFlags.NewTask)
                    .AddCategory(Intent.CategoryHome);
                context.StartActivity(intent);
            }
            catch
            {
            }
        }

        public static string GetAuthority(Context context)
        {
            var packageName = context.ApplicationContext!.PackageName;
            var value = packageName + ".fileProvider";
            return value;
        }

        /// <summary>
        /// 跳转到相机拍照
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="requestCode">返回结果时的请求码</param>
        /// <param name="file">照片存放路径</param>
        public static void StartCameraForResult(Activity activity, int requestCode, JFile? file = null)
        {
            file ??= new JFile(Path.GetTempFileName());
            // https://github.com/xamarin/Essentials/blob/1.5.3.2/Xamarin.Essentials/Types/FileProvider.android.cs
            var imageUri = FileProvider.GetUriForFile(activity, GetAuthority(activity), file);
#if DEBUG
            Android.Util.Log.Debug(nameof(Activity), $"StartCameraForResult imageUri: {imageUri}");
#endif
            var intent = new Intent(MediaStore.ActionImageCapture);
            intent.PutExtra(MediaStore.ExtraOutput, imageUri);
            intent.AddFlags(ActivityFlags.GrantWriteUriPermission);
            activity.StartActivityForResult(intent, requestCode);
        }
    }
}