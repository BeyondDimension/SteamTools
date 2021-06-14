using Android.Runtime;
using Android.Views;
using Binding;
using System.Application.UI.Resx;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(LocalAuthFragment))]
    internal sealed class LocalAuthFragment : BaseFragment<fragment_local_auth>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);
            binding!.textView.Text = GetType().Name.TrimEnd("Fragment") + Environment.NewLine + AppResources.UnderConstruction;
            binding!.textView.Gravity = GravityFlags.Center;
        }
    }
}
