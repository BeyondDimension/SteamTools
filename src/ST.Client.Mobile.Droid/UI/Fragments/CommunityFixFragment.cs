using Android.Runtime;
using Android.Views;
using Binding;
using System.Application.UI.Resx;
using System.Properties;
using System.Windows;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(CommunityFixFragment))]
    internal sealed class CommunityFixFragment : BaseFragment<fragment_community_fix>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_community_fix;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);
            binding!.textView.Text = GetType().Name.TrimEnd("Fragment") + Environment.NewLine + AppResources.UnderConstruction;
            binding!.textView.Gravity = GravityFlags.Center;
        }
    }
}