using Android.Runtime;
using Android.Views;
using Binding;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(GameListFragment))]
    internal sealed class GameListFragment : BaseFragment<fragment_game_list>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_game_list;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);
            binding!.textView.Text = GetType().Name.TrimEnd("Fragment") + Environment.NewLine + AppResources.UnderConstruction;
            binding!.textView.Gravity = GravityFlags.Center;
        }
    }
}
