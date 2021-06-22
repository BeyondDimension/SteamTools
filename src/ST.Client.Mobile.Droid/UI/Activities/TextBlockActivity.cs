using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Binding;
using System.Application.Models;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(TextBlockActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
      LaunchMode = LaunchMode.SingleTask,
      ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class TextBlockActivity : BaseActivity<activity_textblock>
    {
        protected override int? LayoutResource => Resource.Layout.activity_textblock;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var vm = this.GetViewModel<TextBlockViewModel>();
            if (vm == null)
            {
                Finish();
                return;
            }

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            if (!string.IsNullOrWhiteSpace(vm.Title)) Title = vm.Title;

            if (!string.IsNullOrWhiteSpace(vm.Content)) binding!.tvContent.Text = vm.Content;
            else if (vm.ContentSource != default) binding!.tvContent.Text = vm.ContentSource switch
            {
                TextBlockViewModel.ContentSourceEnum.OpenSourceLibrary => OpenSourceLibrary.StringValues,
                _ => string.Empty,
            };
        }

        public static void StartActivity(Activity activity, TextBlockViewModel viewModel)
        {
            GoToPlatformPages.StartActivity<TextBlockActivity, TextBlockViewModel>(activity, viewModel);
        }
    }
}