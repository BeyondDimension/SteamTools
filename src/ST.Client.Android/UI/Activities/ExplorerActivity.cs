using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
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

        protected override ExplorerPageViewModel? OnCreateViewModel()
        {
            var path = this.GetViewModel<string>();
            if (!string.IsNullOrWhiteSpace(path))
            {
                return new(path!);
            }
            return new();
        }

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
            adapter.ItemLongClick += (_, e) => ViewModel!.OnItemLongClick(e.Current);
            binding.rvExplorer.SetLinearLayoutManager();
            binding.rvExplorer.SetAdapter(adapter);
        }

        public override void OnBackPressed()
        {
            if (ViewModel!.OnBack()) return;
            base.OnBackPressed();
        }

        public override bool OnCreateOptionsMenu(IMenu? menu)
        {
            MenuInflater.Inflate(Resource.Menu.explorer_toolbar_menu, menu);
            var menu_add = menu?.FindItem(Resource.Id.menu_add);
            if (menu_add != null)
            {
                ViewModel!.WhenAnyValue(x => x.IsSupportFileCreate).SubscribeInMainThread(value =>
                {
                    menu_add.SetVisible(value);
                }).AddTo(this);
            }
            var menu_delete = menu?.FindItem(Resource.Id.menu_delete);
            var menu_select_all = menu?.FindItem(Resource.Id.menu_select_all);
            var menu_export = menu?.FindItem(Resource.Id.menu_export);
            if (menu_delete != null && menu_select_all != null && menu_export != null)
            {
                ViewModel!.WhenAnyValue(x => x.IsEditMode).SubscribeInMainThread(value =>
                {
                    menu_add?.SetVisible(!value && ViewModel!.IsSupportFileCreate);
                    menu_export.SetVisible(value);
                    menu_delete.SetVisible(value);
                    menu_select_all.SetVisible(value);
                }).AddTo(this);
            }
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_add)
            {
                ViewModel!.ImportFiles();
                return true;
            }
            else if (item.ItemId == Resource.Id.menu_delete)
            {
                ViewModel!.DeletePaths();
                return true;
            }
            else if (item.ItemId == Resource.Id.menu_select_all)
            {
                ViewModel!.SelectOrUnselectAll();
                return true;
            }
            else if (item.ItemId == Resource.Id.menu_export)
            {
                ViewModel!.CopyToPaths();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}