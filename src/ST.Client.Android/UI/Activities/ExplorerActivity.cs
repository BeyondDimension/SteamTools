using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.RecyclerView.Widget;
using Binding;
using ReactiveUI;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(ExplorerActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class ExplorerActivity : BaseActivity<activity_explorer, ExplorerPageViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.activity_explorer;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            ViewModel!.WhenAnyValue(x => x.Title).SubscribeInMainThread(value =>
            {
                Title = value;
            }).AddTo(this);

            var adapter = new ExplorerAdapter(ViewModel!);
            adapter.ItemClick += (_, e) => ViewModel!.OnItemClick(e.Current);
            binding.rvExplorer.SetLinearLayoutManager();
            binding.rvExplorer.SetAdapter(adapter);
        }

        public override void OnBackPressed()
        {
            if (ViewModel!.OnBack()) return;
            base.OnBackPressed();
        }
    }
}