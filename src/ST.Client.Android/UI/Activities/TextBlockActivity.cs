using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using AndroidX.Core.Net;
using Binding;
using System.Application.Models;
using System.Application.UI.ViewModels;
using System.IO;
using static Android.Content.Intent;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(TextBlockActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges,
        Exported = true // Targeting S+ (version 31 and above) requires that an explicit value for android:exported be defined when intent filters are present
        )]
    [IntentFilter(new[] { ActionView }, Categories = new[] { CategoryDefault }, DataMimeType = MediaTypeNames.TXT)]
    internal sealed class TextBlockActivity : BaseActivity<activity_textblock>
    {
        /// <summary>
        /// 最大支持文件大小，25MB
        /// </summary>
        const long MaxFileLength = 26214400;

        protected override int? LayoutResource => Resource.Layout.activity_textblock;

        static string GetContentByFileInfo(FileInfo fileInfo)
        {
            if (!fileInfo.Exists) return $"File {fileInfo.FullName} not found.";
            if (fileInfo.Length > MaxFileLength) return "File with file size greater than 25MB are not supported.";
            var fileContent = File.ReadAllText(fileInfo.FullName);
            return fileContent;
        }

        static string GetContentByFilePath(string filePath) => GetContentByFileInfo(new(filePath));

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            var vm = this.GetViewModel<TextBlockViewModel>();
            if (vm == null)
            {
                if (Intent?.Action == ActionView && Intent.Data != null)
                {
                    try
                    {
                        var filePath = Intent.Data.EnsurePhysicalPath();
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            vm = new TextBlockViewModel
                            {
                                Title = Path.GetFileName(filePath),
                                Content = GetContentByFilePath(filePath!),
                            };
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(nameof(TextBlockActivity), nameof(OnCreate) + "_Intent.Data.EnsurePhysicalPath", e);
                    }
                }
            }

            if (vm == null)
            {
                Finish();
                return;
            }

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            var hasVMTitle = !string.IsNullOrWhiteSpace(vm.Title);
            if (hasVMTitle) Title = vm.Title;

            if (!string.IsNullOrWhiteSpace(vm.Content)) binding!.tvContent.Text = vm.Content;
            else if (!string.IsNullOrEmpty(vm.FilePath))
            {
                try
                {
                    if (!hasVMTitle)
                    {
                        Title = vm.Title = Path.GetFileName(vm.FilePath);
                    }
                    binding!.tvContent.Text = vm.Content = GetContentByFilePath(vm.FilePath!);
                }
                catch (Exception e)
                {
                    Log.Error(nameof(TextBlockActivity), nameof(OnCreate) + "_vm.FilePath", e);
                }
            }
            else if (vm.ContentSource != default) binding!.tvContent.Text = vm.ContentSource switch
            {
                TextBlockViewModel.ContentSourceEnum.OpenSourceLibrary => OpenSourceLibrary.StringValues,
                TextBlockViewModel.ContentSourceEnum.Translators => AboutPageViewModel.TextTranslators,
                _ => string.Empty,
            };

            if (vm.FontSizeResId.HasValue)
            {
                binding!.tvContent.SetTextSize(ComplexUnitType.Px, Resources!.GetDimension(vm.FontSizeResId.Value));
            }
        }

        public static void StartActivity(Activity activity, TextBlockViewModel viewModel)
        {
            GoToPlatformPages.StartActivity<TextBlockActivity, TextBlockViewModel>(activity, viewModel);
        }
    }
}